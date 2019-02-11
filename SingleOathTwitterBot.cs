using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Text;
using System.Linq;
using System.Net.Http;

//Warning: this class should work but I have no way to test it because I need a twitter developer account to do so
/*
 * I'm using Oath because I'm already familiar with it and don't want to waste too much time on this bonus, there's also good Twitter documentation about it
 * For the same reason, I'm doing "Single Oath" instead of a full oath implementation.
 * More info about this here : https://developer.twitter.com/en/docs/basics/authentication/overview/oauth
 * 
 * Also, I'm over-commenting because several members of the class said that they wanted to read my code once I'm done with it
 * 
 */
namespace MarkovChain_ArcaniteSauce
{
    public class SingleOathTwitterBot
    {
        private static string twitterAPIURL = "https://api.twitter.com/1.1/";

        //The following is an improved version of the Random() object. Since this bot is not important, I'm not sure it really is useful here, but too much security is better than not enough
        private static RNGCryptoServiceProvider rngCsp = new RNGCryptoServiceProvider(); //doc: https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.rngcryptoserviceprovider?view=netframework-4.7.2

        //The following properties are needed for Oauth
        private string consumerKey;
        private string oauthToken;

        //The following properties are useful to generate the sigHash
        private string signatureKey;
        private string signatureToken;

        //The following properties are imposed by Twitter
        private DateTime epochUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc); // date of the Unix epoch
        private HMACSHA1 signatureMaker; //HMACSHA1 serves to generate signature using the SHA1 hashing algorithm

        public SingleOathTwitterBot(string consKey, string oaToken, string sigKey, string sigToken)
        {
            this.consumerKey = consKey;
            this.oauthToken = oaToken;
            this.signatureKey = sigKey;
            this.signatureToken = sigToken;

            //if you need explanations for the following line, either ask me or read here https://docs.microsoft.com/en-us/dotnet/api/system.security.cryptography.hmacsha256?view=netframework-4.7.2
            signatureMaker = new HMACSHA1(new ASCIIEncoding().GetBytes(string.Format("{0}&{1}", this.signatureKey, this.signatureToken)));
        }

        //A task is used to write asynchronous logics. If you don't know what that means, ask me or read here https://en.wikipedia.org/wiki/Asynchrony_(computer_programming)
        Task<string> SendRequest(string url, Dictionary<string, string> data)
        {
            string completeURL = twitterAPIURL + url;

            
            data = GeneratePostData(data, completeURL);

            string oAuthHeader = GenerateOAuthHeader(data);

            // Build the form data (exclude OAuth stuff that's already in the header).
            var formData = new FormUrlEncodedContent(data.Where(dic => !dic.Key.StartsWith("oauth_"))); //more about FormUrlEncodedContent here https://docs.microsoft.com/en-us/previous-versions/visualstudio/hh158958(v%3Dvs.118); you can also find more about HTTP requests here: https://en.wikipedia.org/wiki/Hypertext_Transfer_Protocol#Request_message

            return SubmitRequest(completeURL, oAuthHeader, formData);
        }
        private Dictionary<string, string> GeneratePostData(Dictionary<string, string> postData, string url)
        {
            //Just building the HTTP request, the code is quite self explanatory

            int timestamp = (int)((DateTime.UtcNow - epochUtc).TotalSeconds); //number of seconds since the Unix epoch time

            postData.Add("oauth_consumer_key", this.consumerKey);
            postData.Add("oauth_signature_method", "HMAC-SHA1");
            postData.Add("oauth_timestamp", timestamp.ToString());
            postData.Add("oauth_nonce", GenerateNonce());
            postData.Add("oauth_token", oauthToken);
            postData.Add("oauth_version", "1.0");
            postData.Add("oauth_signature", GenerateSignature(url, postData));

            return postData;
        }
        public async Task<string> SendATweet(string tweetedTxt)
        {
            //read about json: https://en.wikipedia.org/wiki/JSON
            Dictionary<string, string> postData = new Dictionary<string, string>
            {
                {"status", tweetedTxt },
                {"trim_user", "1" }
            };
            
            return await SendRequest("statuses/update.json", postData);
        }

        public string GenerateNonce()
        {
            // A nonce is a key used only once to let Twitter now that this request is different than another otherwise identical one
            //This method outputs nonces that are similiar to the ones showed on Twitter's page, even though that isn't mandatory.
            //The use of cryptic random is a bit of an overkill. Twitter says on its page that "any approach which produces a relatively random alphanumeric string should be OK here."
            byte[] randomNumber = new byte[32];
            return Convert.ToBase64String(randomNumber);
        }

        string GenerateSignature(string url, Dictionary<string, string> data)
        {
            string signatureRoot = string.Join(
                "&",
                data
                    .Union(data)
                    .Select(kvp => string.Format("{0}={1}", Uri.EscapeDataString(kvp.Key), Uri.EscapeDataString(kvp.Value)))
                    .OrderBy(s => s)
            );

            string signatureData = string.Format(
                "{0}&{1}&{2}",
                "POST",
                Uri.EscapeDataString(url),
                Uri.EscapeDataString(signatureRoot)//
            );

            return Convert.ToBase64String(signatureMaker.ComputeHash(new ASCIIEncoding().GetBytes(signatureData/**/)));
        }

        string GenerateOAuthHeader(Dictionary<string, string> data)
        {
            string WeirdlySQLAlike = string.Join(
                ", ",
                data
                    .Where(dic => dic.Key.StartsWith("oauth_"))
                    .Select(dic => string.Format("{0}=\"{1}\"", Uri.EscapeDataString(dic.Key), Uri.EscapeDataString(dic.Value))) //Be careful, Uri.EscapeDataString is the only way to escape data that Twitter accepts
                    .OrderBy(s => s)
            );
            return "OAuth " + WeirdlySQLAlike;
        }

         async Task<string> SubmitRequest(string destinationURL, string oAuthHeader, FormUrlEncodedContent postData) //async allows the use of the "await" keyword
        {
            using (HttpClient http = new HttpClient()) //this is basically the same as a streamreader
            {
                //I believe the following code is self explanatory, but don't hesitate to ask me if you have any question
                http.DefaultRequestHeaders.Add("Authorization", oAuthHeader);

                HttpResponseMessage httpResponse = await http.PostAsync(destinationURL, postData);

                return await httpResponse.Content.ReadAsStringAsync();
            }
        }
    }
}
