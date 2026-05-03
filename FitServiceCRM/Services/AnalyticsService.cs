using FitServiceCRM.Data;
using FitServiceCRM.Models;
using Microsoft.EntityFrameworkCore;

namespace FitServiceCRM.Services;

public sealed class AnalyticsService(AppDbContext db)
{
    public async Task<AnalyticsResult> BuildAsync(int? serviceId, int? employeeId, DateTime? from, DateTime? to)
    {
        var query = db.WorkOrders
            .AsNoTracking()
            .Include(order => order.OrderServices).ThenInclude(orderService => orderService.ServiceItem)
            .Include(order => order.Executors).ThenInclude(executor => executor.Employee)
            .Where(order => order.Status == WorkOrderStatus.Completed);

        if (serviceId is not null)
        {
            query = query.Where(order => order.OrderServices.Any(orderService => orderService.ServiceItemId == serviceId));
        }

        if (employeeId is not null)
        {
            query = query.Where(order => order.Executors.Any(executor => executor.EmployeeId == employeeId)
                || order.ReceiverEmployeeId == employeeId);
        }

        if (from is not null)
        {
            query = query.Where(order => order.PlannedStartAt >= from.Value);
        }

        if (to is not null)
        {
            query = query.Where(order => order.PlannedStartAt <= to.Value);
        }

        var orders = await query.OrderBy(order => order.PlannedStartAt).ToListAsync();

        var groups = orders
            .GroupBy(order => new DateTime(order.PlannedStartAt.Year, order.PlannedStartAt.Month, 1))
            .OrderBy(group => group.Key)
            .Select((group, index) => new AnalyticsPoint
            {
                Index = index + 1,
                Label = group.Key.ToString("MM.yyyy"),
                OrdersCount = group.Count(),
                Revenue = group.Sum(order => order.TotalCost)
            })
            .ToList();

        var x = groups.Select(point => (double)point.Index).ToArray();
        var y = groups.Select(point => (double)point.Revenue).ToArray();
        var countY = groups.Select(point => (double)point.OrdersCount).ToArray();

        var (intercept, slope) = LinearRegression(x, y);
        var (countIntercept, countSlope) = LinearRegression(x, countY);

        foreach (var point in groups)
        {
            point.TrendRevenue = (decimal)Math.Max(0, intercept + slope * point.Index);
        }

        var meanDemand = countY.Length == 0 ? 0 : countY.Average();
        var varianceDemand = countY.Length == 0 ? 0 : countY.Sum(value => Math.Pow(value - meanDemand, 2)) / countY.Length;
        var forecastIndex = groups.Count + 1;
        var forecastOrders = Math.Max(0, countIntercept + countSlope * forecastIndex);
        var forecastRevenue = Math.Max(0, intercept + slope * forecastIndex);
        var averageCheck = orders.Count == 0 ? 0 : orders.Average(order => order.TotalCost);

        return new AnalyticsResult
        {
            Points = groups,
            OrdersCount = orders.Count,
            TotalRevenue = orders.Sum(order => order.TotalCost),
            AverageCheck = averageCheck,
            LeastSquaresSlope = slope,
            MomentsMeanDemand = meanDemand,
            MomentsVarianceDemand = varianceDemand,
            MaximumLikelihoodLambda = meanDemand,
            ForecastOrders = forecastOrders,
            ForecastRevenue = (decimal)forecastRevenue,
            Interpretation = BuildInterpretation(orders.Count, slope, meanDemand, varianceDemand, forecastOrders, forecastRevenue)
        };
    }

    private static (double Intercept, double Slope) LinearRegression(double[] x, double[] y)
    {
        if (x.Length == 0 || y.Length == 0)
        {
            return (0, 0);
        }

        if (x.Length == 1)
        {
            return (y[0], 0);
        }

        var xMean = x.Average();
        var yMean = y.Average();
        var numerator = x.Zip(y, (xValue, yValue) => (xValue - xMean) * (yValue - yMean)).Sum();
        var denominator = x.Sum(xValue => Math.Pow(xValue - xMean, 2));
        var slope = denominator == 0 ? 0 : numerator / denominator;
        var intercept = yMean - slope * xMean;
        return (intercept, slope);
    }

    private static string BuildInterpretation(int ordersCount, double slope, double meanDemand, double varianceDemand, double forecastOrders, double forecastRevenue)
    {
        if (ordersCount == 0)
        {
            return "Для выбранного периода нет завершенных работ. Измените фильтры, чтобы построить прогноз.";
        }

        var trend = slope >= 0 ? "положительный" : "отрицательный";
        var volatility = varianceDemand > meanDemand ? "спрос нестабилен, стоит держать резерв слотов" : "спрос относительно ровный";

        return $"МНК показывает {trend} тренд выручки. По методу моментов средний спрос составляет {meanDemand:N1} заказ-наряда в месяц; {volatility}. Оценка ММП для пуассоновской интенсивности равна {meanDemand:N1}, прогноз на следующий период: {forecastOrders:N1} заказ-наряда и {forecastRevenue:N0} ₽ выручки.";
    }
}

public sealed class AnalyticsResult
{
    public List<AnalyticsPoint> Points { get; set; } = [];
    public int OrdersCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageCheck { get; set; }
    public double LeastSquaresSlope { get; set; }
    public double MomentsMeanDemand { get; set; }
    public double MomentsVarianceDemand { get; set; }
    public double MaximumLikelihoodLambda { get; set; }
    public double ForecastOrders { get; set; }
    public decimal ForecastRevenue { get; set; }
    public string Interpretation { get; set; } = string.Empty;
}

public sealed class AnalyticsPoint
{
    public int Index { get; set; }
    public string Label { get; set; } = string.Empty;
    public int OrdersCount { get; set; }
    public decimal Revenue { get; set; }
    public decimal TrendRevenue { get; set; }
}
