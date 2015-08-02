using BatcoinMarket.Business;
using System.Web.Mvc;

namespace BatcoinMarket.InsecureFormatters
{
    public static class InsecureHtmlHelpers
    {
        public static MvcHtmlString FormatUserName(this HtmlHelper html, Account account)
        {
            var tagBuilder = new TagBuilder("li");
            tagBuilder.InnerHtml = string.Format("{0} - {1}", account.Username, account.Balance);
            return MvcHtmlString.Create(tagBuilder.ToString());
        }
    }
}