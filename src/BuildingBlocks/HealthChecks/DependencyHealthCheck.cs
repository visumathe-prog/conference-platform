public class DependencyHealthCheck : IHealthCheck
{
    private readonly IConfiguration _configuration;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var issues = new List<string>();
        
        // Check each dependency
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY")))
            issues.Add("Stripe API key missing");
        
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SENDGRID_API_KEY")))
            issues.Add("SendGrid API key missing");
        
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("AWS_ACCESS_KEY_ID")))
            issues.Add("AWS credentials missing");
        
        if (issues.Any())
        {
            return HealthCheckResult.Degraded(
                "Some dependencies are missing credentials",
                data: new Dictionary<string, object> { { "Missing", issues } });
        }
        
        return HealthCheckResult.Healthy("All credentials configured");
    }
}
