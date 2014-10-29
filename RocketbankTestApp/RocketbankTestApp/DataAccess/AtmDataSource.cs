using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace RocketbankTestApp.DataAccess
{
    public class AtmDataSource
    {
        #region Nested classes
        private DateTime FromUTS(int timeStemp)
        {
            System.DateTime dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            return dateTime.AddSeconds(timeStemp);
        }

        #endregion
       

        private const string TEST_FILE_NAME = "cashin.json";

        private DateTime lastUpdate;

        public DateTime LastUpdate
        {
            get { return lastUpdate; }
            set { lastUpdate = value; }
        }

        public async Task<List<Models.Atm>> GetATMs()
        {
            List<Models.Atm> models = new List<Models.Atm>();
            StorageFolder folder = Windows.ApplicationModel.Package.Current.InstalledLocation;
            using (Stream stream = await folder.OpenStreamForReadAsync(TEST_FILE_NAME))
            using (StreamReader reader = new StreamReader(stream))
            {
                var jsonString = await reader.ReadToEndAsync();
                Models.WebResponce responce = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.WebResponce>(jsonString);
                if (responce!=null)
                {
                    this.LastUpdate = FromUTS(responce.LastUpdate);
                    models.AddRange(responce.Atms);
                    System.Diagnostics.Debug.WriteLine("Received " + models.Count + " atm records");
                }
            }
            return models;
        }
    }
}
