namespace Bipins.Trading.Indicators.Utilities;

/// <summary>
/// Span-based math helpers for indicator hot paths. No LINQ, no extra allocations.
/// </summary>
public static class SpanMath
{
    /// <summary>Sum of values in span.</summary>
    public static double Sum(ReadOnlySpan<double> span)
    {
        double sum = 0;
        for (int i = 0; i < span.Length; i++)
            sum += span[i];
        return sum;
    }

    /// <summary>Mean (average) of values.</summary>
    public static double Mean(ReadOnlySpan<double> span)
    {
        if (span.Length == 0) return 0;
        return Sum(span) / span.Length;
    }

    /// <summary>Variance (population variance: sum of squared deviations / n).</summary>
    public static double Variance(ReadOnlySpan<double> span, bool sampleVariance = false)
    {
        if (span.Length == 0) return 0;
        double mean = Mean(span);
        double sumSq = 0;
        for (int i = 0; i < span.Length; i++)
        {
            double d = span[i] - mean;
            sumSq += d * d;
        }
        int n = span.Length;
        return sampleVariance && n > 1 ? sumSq / (n - 1) : sumSq / n;
    }

    /// <summary>Standard deviation (population).</summary>
    public static double StdDev(ReadOnlySpan<double> span, bool sample = false)
    {
        return Math.Sqrt(Variance(span, sample));
    }

    /// <summary>Minimum value in span.</summary>
    public static double Min(ReadOnlySpan<double> span)
    {
        if (span.Length == 0) return double.NaN;
        double min = span[0];
        for (int i = 1; i < span.Length; i++)
            if (span[i] < min) min = span[i];
        return min;
    }

    /// <summary>Maximum value in span.</summary>
    public static double Max(ReadOnlySpan<double> span)
    {
        if (span.Length == 0) return double.NaN;
        double max = span[0];
        for (int i = 1; i < span.Length; i++)
            if (span[i] > max) max = span[i];
        return max;
    }

    /// <summary>Linear regression: slope and intercept for y = slope * x + intercept (x = 0,1,2,...).</summary>
    public static (double slope, double intercept) LinearRegression(ReadOnlySpan<double> y)
    {
        int n = y.Length;
        if (n < 2) return (0, n == 1 ? y[0] : 0);
        double sumX = 0, sumY = 0, sumXY = 0, sumX2 = 0;
        for (int i = 0; i < n; i++)
        {
            sumX += i;
            sumY += y[i];
            sumXY += i * y[i];
            sumX2 += i * i;
        }
        double denom = n * sumX2 - sumX * sumX;
        if (Math.Abs(denom) < 1e-20) return (0, Mean(y));
        double slope = (n * sumXY - sumX * sumY) / denom;
        double intercept = (sumY - slope * sumX) / n;
        return (slope, intercept);
    }
}
