using Microsoft.Win32;
using OfficeOpenXml.ConditionalFormatting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace HM
{
    internal class Post
    {

        private string Auth_PM;//поле заполниться при вызове класса
        private string Auth_Engy;//поле заполниться при вызове класса
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

        #region PM


        /// <summary>
        ///Метод обновления вгх
        /// </summary>
        /// <param name="RP">RP дочки (только цифры!)</param>
        public void UpdateVGH(string RP, TextBlock tb)
        {
            var data = $@"{{
                        ""id"": ""61714cec372bd5.97068872"",
                        ""method"": ""postamat.updateParcel"",
                        ""params"": [
                            {RP}
                        ],
                        ""jsonrpc"": ""2.0""
                    }}";

            SendPostRequest(api_Shiptor, data, tb);

        }

        /// <summary>
        /// Отвязать от постомата
        /// </summary>
        /// <param name="RP">Посылка дочь</param>
        /// <returns></returns>
        public void unlinkPackage(string RP, TextBlock tb)
        {
            var data = $@"{{
                        ""id"": ""JsonRpcClient.js"",
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""postamat.unlinkPackage"",
                        ""params"": [
                            {RP}
                        ]
                    }}";
            SendPostRequest(api_Shiptor, data, tb);
        }

        /// <summary>
        /// 1 бронь
        /// </summary>
        /// <param name="RP">Посылка дочь</param>
        /// <returns></returns>
        public void enqueue(string RP, TextBlock tb)
        {
            var data = $@"{{
                      ""id"": ""JsonRpcClient.js"",
                      ""jsonrpc"": ""2.0"",
                      ""method"": ""postamat.enqueue"",
                      ""params"": [{RP}]" +
                      "}}";
            SendPostRequest(api_Shiptor, data, tb);
        }

        /// <summary>
        /// 2 бронь
        /// </summary>
        /// <param name="RP">Посылка дочь</param>
        /// <returns></returns>
        public void bookDestinationCell(string RP, TextBlock tb)
        {
            var data = $@"{{
                        ""id"": ""JsonRpcClient.js"",
                        ""jsonrpc"": ""2.0"",
                        ""method"": ""postamat.bookDestinationCell"",
                        ""params"": [{RP}]
                    }}";
            SendPostRequest(api_Shiptor, data, tb);
        }

        public void Load_PM_CELL(string PS, int cell, int lastEventId, TextBlock tb)
        {
            var data = $@"{{
""id"": ""JsonRpcClient.js"",
""jsonrpc"": ""2.0"",
""method"": ""syncState"",
""params"": {{
""time"": ""2021-04-29 13:01:10"",
""postamat_sn"": ""{PS}"",
""message_sn"": 0,
""system"": {{
""cpu_id"": ""54502DC6-EC031104-DC8447C8"",
""flash_id"": ""116861C6-2F4E3B03"",
""gsm_IMEI"": ""867556042597123"",
""gsm_IMSI"": ""250991623985898"",
""gsm_ICCID"": ""8970199190844163853"",
""gsm_revision"": ""1418B08SIM800C24_TLS12"",
""gsm_signal"": {{
""ber"": 0,
""dBm"": -52,
""measure_time"": ""2021-04-26 12:24:50""
}},
""bluetooth_MAC"": ""38:1c:4a:2c:44:da"",
""hardware_version"": ""01.01.02.01"",
""software_version"": ""01.01.02.06""
}},
""events"": [
{{
""time"": ""2021-04-29 13:01:10"",
""id"": {lastEventId},
""event"": ""load"",
""param"": {{
""cell"": {cell}
}}
}}
]
}}
}}";
            tb.Text += "\n <PM> В ожидает выдачи:\n";
            SendPostRequest("https://pm.shiptor.ru/api/postamat/v3", data, Auth_PM, tb);
            tb.Text += "\n";

        }

        #endregion



        #region engy
        /// <summary>
        /// В новую из ожидает +
        /// </summary>
        /// <param name="RP">посылка дочка</param>
        /// <param name="tb">для лога</param>
        public void InNew_Engy_Clild(string RP, TextBlock tb)
        {
            var data = $@"{{
        ""ItemExternalId"": ""{RP}""  //Место
}}";
            tb.Text += "\n <ENGY> В новую из ожидает:\n";
            SendPostRequest("https://pst-app.sblogistica.ru/supportapi/InvoiceB2CFromDropToNew", data, Auth_Engy, tb);
            tb.Text += "\n";
        }

        /// <summary>
        /// смена яч у заброненной
        /// </summary>
        /// <param name="RP">посылка дочка</param>
        /// <param name="cell">Ячейка из Engy</param>
        /// <param name="tb">для лога</param>
        public void ChancheCell_Engy_Child(string RP, int cell, TextBlock tb)
        {
            var data = $@"{{
  ""ItemExternalId"": ""{RP}"", 
  ""CellName"": ""{cell}""
}}";
            tb.Text += "\n <ENGY> Смена ячейки на " + cell.ToString() + ":\n";
            SendPostRequest("https://pst-app.sblogistica.ru/supportapi/InvoiceBookedOrDroppedChangeCell", data, Auth_Engy, tb);
            tb.Text += "\n";
        }

        /// <summary>
        /// В заложено постамамты 2.1
        /// </summary>
        /// <param name="RP">посылка дочка</param>
        /// <param name="tb">для лога</param>
        public void InLoad_Engy_Clild(string RP, TextBlock tb)
        {
            var data = $@"{{
   ""ItemExternalId"": ""544610053"" 
}}";
            tb.Text += "\n <ENGY> В заложено:\n";
            SendPostRequest("https://pst-app.sblogistica.ru/supportapi/InvoiceFromBookToDrop", data, Auth_Engy, tb);
            tb.Text += "\n";
        }

        #endregion




        #endregion

        #region Goups_Methods

        public void MethodsSynchr_Engy(string RP, int cell, TextBlock tb)
        { ///1 В новую из ожидает +
          ///2.Смена яч у заброненной на 0, потом на нужную из поля 
          ///3.В заложено постоматы 2.1}
            InNew_Engy_Clild(RP, tb);
            ChancheCell_Engy_Child(RP, 0, tb);
            ChancheCell_Engy_Child(RP, cell, tb);
            InLoad_Engy_Clild(RP, tb);

        }

        #endregion

        #region Отправка POST
        /// <summary>
        ///Отправка POST-запроса
        /// </summary>
        /// <param name="url">URL-адрес сервера</param>
        /// <param name="jsonRequest">Тело запроса JSON</param>
        /// <returns></returns>
        /*  async void SendPostRequest(string url, string jsonRequest, TextBlock tb)
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

                          tb.Text += await response.Content.ReadAsStringAsync();

                      }
                      else
                      {

                          var responseContent = await response.Content.ReadAsStringAsync();
                          tb.Text += $@"Запрос вернул ответ с ошибкой: {response.StatusCode}; Error message: {responseContent}";

                      }
                      //return await response.Content.ReadAsStringAsync();
                  }
              }
          }*/

        void SendPostRequest(string url, string jsonRequest, TextBlock tb)
        {
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(jsonRequest);
                httpContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");

                var response = httpClient.PostAsync(url, httpContent).Result; // Используем .Result для блокировки и получения результата синхронно

                if (response.IsSuccessStatusCode)
                {
                    //MessageBox.Show(response.Content.ReadAsStringAsync().Result); // Синхронное получение и отображение ответа
                    tb.Text += response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    tb.Text += $@"Запрос вернул ответ с ошибкой: {response.StatusCode}; Error message: {responseContent}";
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
        /*  async void SendPostRequest(string url, string jsonRequest, string apiKey, TextBlock tb)
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
                          tb.Text += await response.Content.ReadAsStringAsync();

                      }
                      else
                      {
                          var responseContent = await response.Content.ReadAsStringAsync();
                          tb.Text += $@"Запрос вернул ответ с ошибкой: {response.StatusCode}; Error message: {responseContent}";

                      }
                  }
              }
          }*/
        void SendPostRequest(string url, string jsonRequest, string apiKey, TextBlock tb)
        {
            using (var httpClient = new HttpClient())
            {
                var httpContent = new StringContent(jsonRequest);
                httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                httpClient.DefaultRequestHeaders.Add("X-AUTH-TOKEN", $"Bearer {apiKey}");

                var response = httpClient.PostAsync(url, httpContent).Result; // Используем .Result для блокировки и получения результата синхронно

                if (response.IsSuccessStatusCode)
                {
                    tb.Text += response.Content.ReadAsStringAsync().Result;
                }
                else
                {
                    var responseContent = response.Content.ReadAsStringAsync().Result;
                    tb.Text += $@"Запрос вернул ответ с ошибкой: {response.StatusCode}; Error message: {responseContent}";
                }
            }
        }



        #endregion



    }
}


