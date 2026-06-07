using Stripe;
using Microsoft.Extensions.Options;

namespace Conference.Payment.Service.Services
{
    /// <summary>
    /// Real Stripe integration - requires valid API key in .env
    /// If key is invalid, Stripe SDK will throw AuthenticationException
    /// </summary>
    public class StripePaymentService : IPaymentService
    {
        private readonly StripeSettings _settings;
        private readonly ILogger<StripePaymentService> _logger;
        
        public StripePaymentService(
            IOptions<StripeSettings> settings,
            ILogger<StripePaymentService> logger)
        {
            _settings = settings.Value;
            _logger = logger;
            
            // Set Stripe API key from environment
            StripeConfiguration.ApiKey = _settings.SecretKey;
            
            _logger.LogInformation("Stripe service initialized with key: {KeyPrefix}", 
                _settings.SecretKey?[..8]); // Log only first 8 chars for debugging
        }
        
        /// <summary>
        /// Create a payment intent for conference registration
        /// Returns: PaymentIntentId and ClientSecret for frontend confirmation
        /// </summary>
        public async Task<PaymentResult> CreatePaymentIntentAsync(
            decimal amount, 
            string currency, 
            string conferenceId,
            string attendeeEmail)
        {
            try
            {
                var options = new PaymentIntentCreateOptions
                {
                    Amount = (long)(amount * 100), // Convert to cents
                    Currency = currency,
                    Metadata = new Dictionary<string, string>
                    {
                        { "conference_id", conferenceId },
                        { "attendee_email", attendeeEmail }
                    },
                    ReceiptEmail = attendeeEmail,
                    PaymentMethodTypes = new List<string> { "card" }
                };
                
                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);
                
                _logger.LogInformation(
                    "Payment intent created: {IntentId} for conference {ConferenceId}",
                    paymentIntent.Id, conferenceId);
                
                return PaymentResult.Success(
                    paymentIntent.Id,
                    paymentIntent.ClientSecret);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, 
                    "Stripe payment failed: {StripeError}. Check STRIPE_SECRET_KEY in .env",
                    ex.StripeError?.Message);
                
                return PaymentResult.Failure($"Payment failed: {ex.Message}");
            }
        }
        
        /// <summary>
        /// Handle Stripe webhook (real signature verification)
        /// </summary>
        public async Task<WebhookResult> HandleWebhookAsync(
            string jsonPayload,
            string stripeSignature)
        {
            try
            {
                var stripeEvent = EventUtility.ConstructEvent(
                    jsonPayload,
                    stripeSignature,
                    _settings.WebhookSecret);
                
                switch (stripeEvent.Type)
                {
                    case Events.PaymentIntentSucceeded:
                        var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                        await HandleSuccessfulPayment(paymentIntent);
                        break;
                        
                    case Events.PaymentIntentPaymentFailed:
                        var failedPayment = stripeEvent.Data.Object as PaymentIntent;
                        await HandleFailedPayment(failedPayment);
                        break;
                }
                
                return WebhookResult.Success();
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, 
                    "Webhook signature verification failed. Check STRIPE_WEBHOOK_SECRET");
                return WebhookResult.Failure("Invalid signature");
            }
        }
    }
}
