using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace HM
{
    internal class Post
    {

        private string Auth_PM;
        private string Auth_Engy;
        private string api_Shiptor = "https://api.shiptor.ru/system/v1?key=SemEd5DexEk7Ub2YrVuyNavNutEh4Uh8TerSuwEnMev";
        private string api_PM;

        public Post()
        {
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\HM\Settings"))
            {
                if (key != null)
                {
                    Auth_PM = key.GetValue("X-AUTH-TOKEN_PM")?.ToString();
                    Auth_Engy = key.GetValue("X-AUTH-TOKEN_Engy")?.ToString();
                }
            }
        }

        #region Methods

        /// <summary>
        ///Метод обновления вгх
        /// </summary>
        /// <param name="RP">RP дочки (только цифры!)</param>
        public async Task<string> UpdateVGH(string RP)
        {
            var data = $@"{{
                        ""id"": ""61714cec372bd5.97068872"",
                        ""method"": ""postamat.updateParcel"",
                        ""params"": [
                            {RP}
                        ],
                        ""jsonrpc"": ""2.0""
                    }}";

            return await SendPostRequest(api_Shiptor, data);

        }

        /// <summary>
        /// Отвязать от постомата
        /// </summary>
        /// <param name="RP">Посылка дочь</param>
        /// <returns></returns>
        public async Task<string> unlinkPackage(string RP)
        {
            var data = $@"{{
                        ""id"": ""JsonRpcClient.js"",
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""postamat.unlinkPackage"",
                        ""params"": [
                            {RP}
                        ]
                    }}";
            return await SendPostRequest(api_Shiptor, data);
        }

        /// <summary>
        /// 1 бронь
        /// </summary>
        /// <param name="RP">Посылка дочь</param>
        /// <returns></returns>
        public async Task<string> enqueue(string RP)
        {
            var data = $@"{{
                      ""id"": ""JsonRpcClient.js"",
                      ""jsonrpc"": ""2.0"",
                      ""method"": ""postamat.enqueue"",
                      ""params"": [{RP}]" +
                      "}}";
            return await SendPostRequest(api_Shiptor, data);
        }

        /// <summary>
        /// 2 бронь
        /// </summary>
        /// <param name="RP">Посылка дочь</param>
        /// <returns></returns>
        public async Task<string> bookDestinationCell(string RP)
        {
            var data = $@"{{
                        ""id"": ""JsonRpcClient.js"",
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""postamat.bookDestinationCell"",
                        ""params"": [{RP}]
                    }}";
            return await SendPostRequest(api_Shiptor, data);
        }



        #endregion

        #region Отправка POST
        /// <summary>
        ///Отправка POST-запроса
        /// </summary>
        /// <param name="url">URL-адрес сервера</param>
        /// <param name="jsonRequest">Тело запроса JSON</param>
        /// <returns></returns>
        static async Task<string> SendPostRequest(string url, string jsonRequest)
        {
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(jsonRequest);
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                using (var response = await httpClient.PostAsync(url, httpContent))
                {
                    if (response.IsSuccessStatusCode)
                    {

                        MessageBox.Show(await response.Content.ReadAsStringAsync());
                        ///получать сам ответ от запроса для логирования

                        return await response.Content.ReadAsStringAsync();

                    }
                    else
                    {

                        var responseContent = await response.Content.ReadAsStringAsync();
                        return $@"Запрос вернул ответ с ошибкой: {response.StatusCode}; Error message: {responseContent}";

                    }
                    //return await response.Content.ReadAsStringAsync();
                }
            }
        }
        /// <summary>
        /// Отправка POST-запросов с ключом доступа
        /// </summary>
        /// <param name="url">URL-адрес сервера</param>
        /// <param name="jsonRequest">Тело запроса JSON</param>
        /// <param name="apiKey">Токен доступа</param>
        /// <returns></returns>
        static async Task<string> SendPostRequest(string url, string jsonRequest, string apiKey)
        {
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(jsonRequest);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                httpClient.DefaultRequestHeaders.Add("X-AUTH-TOKEN", $"Bearer {apiKey}");

                using (var response = await httpClient.PostAsync(url, httpContent))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        return await response.Content.ReadAsStringAsync();

                    }
                    else
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        return $@"Запрос вернул ответ с ошибкой: {response.StatusCode}; Error message: {responseContent}";

                    }
                }
            }
        }
        #endregion



    }
}


