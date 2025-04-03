using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace BinusSchool.I18n.Extensions
{
    public static class HttpContextExtension
    {
        private static IEnumerable<IRequestCultureProvider> _requestCultureProviders;
        
        public static async Task DetermineLocalization(this HttpContext context)
        {
            _requestCultureProviders = _requestCultureProviders ?? new IRequestCultureProvider[]
            {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            };

            var selectedCulture = CultureInfo.InvariantCulture;
            var selectedUICulture = CultureInfo.InvariantCulture;
            
            foreach (var requestCultureProvider in _requestCultureProviders)
            {
                var result = await requestCultureProvider.DetermineProviderCultureResult(context);
                if (result is null)
                    continue;

                var culture = result.Cultures.FirstOrDefault().Value;
                if (!string.IsNullOrEmpty(culture))
                    selectedCulture = new CultureInfo(culture);
                
                var uiCulture = result.UICultures.FirstOrDefault().Value;
                if (!string.IsNullOrEmpty(uiCulture))
                    selectedUICulture = new CultureInfo(uiCulture);
            }

            // Thread.CurrentThread.CurrentCulture = selectedCulture;
            CultureInfo.CurrentCulture = selectedCulture;
            CultureInfo.DefaultThreadCurrentCulture = selectedCulture;

            // Thread.CurrentThread.CurrentUICulture = selectedUICulture;
            CultureInfo.CurrentUICulture = selectedUICulture;
            CultureInfo.DefaultThreadCurrentUICulture = selectedUICulture;
        }
    }
}