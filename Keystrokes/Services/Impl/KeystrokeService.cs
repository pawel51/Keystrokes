using CsvHelper;
using CsvHelper.Configuration;
using Keystrokes.Models;
using KeystrokesData;
using KeystrokesData.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Keystrokes.Services
{
    public class KeystrokeService : IKeystrokeService
    {
        private readonly KeystrokesDbContext context;

        public KeystrokeService(KeystrokesDbContext context)
        {
            this.context = context;
        }

        public void Hello()
        {
            List<TestSample> s = context.TestData.ToList();
            Console.WriteLine("Hello");
        }

        public TrainSample? AddTrainSample(Dictionary<string, List<(double flight, double dwell)>> probe, string categoryName)
        {
            // { "znak": (mean flight, mean dwell)
            Dictionary<string, (double flight, double dwell)> meanProbe 
                = new Dictionary<string, (double flight, double dwell)>();

            // convert to meanProbe
            probe.ToList().ForEach(p =>
            {
                double sumFlight = 0, sumDwell = 0;
                p.Value.ForEach((item) =>
                {
                    sumFlight += item.flight;
                    sumDwell += item.dwell;
                });
                double meanFlight = sumFlight / p.Value.Count;
                double meanDwell = sumDwell / p.Value.Count;
                meanProbe.Add(p.Key, (meanFlight, meanDwell));
            });

            List<SingleProbe> keyList = new List<SingleProbe>();

            meanProbe.ToList().ForEach(p =>
            {
                keyList.Add(new SingleProbe()
                {
                    AsciiSign = p.Key,
                    Dwell = p.Value.dwell,
                    Flight = p.Value.flight
                });
            });

            TrainSample sample = new TrainSample()
            {
                Category = categoryName,
                Probes = keyList
            };

            // check if category exists
            if (context.TrainData.FirstOrDefault(e => e.Category == categoryName) != null)
            {
                context.TrainData.Update(sample);
            }
            else
            {
                context.TrainData.Add(sample);
            }
            
            context.SaveChanges();

            return sample;
        }

        public List<TrainSample> GetTrainSamples()
        {
            return context.TrainData.Include(m => m.Probes).ToList();
        }

        public List<TrainSample> ReadTreningData(string testDataFileName)
        {
            List<TrainSample> trainSamples = new List<TrainSample>();


            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = false,
                MissingFieldFound = null
            };
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult result = fbd.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    SetupCategories(testDataFileName, config, fbd, trainSamples);
            }

            return trainSamples;
        }

        private void SetupCategories(string testDataFileName, CsvConfiguration config, FolderBrowserDialog fbd, List<TrainSample> trainSamples)
        {
            
            string[] files = Directory.GetFiles(fbd.SelectedPath);

            List<SingleRowModel>[] sfm = new List<SingleRowModel>[files.Length];

            for (int i = 0; i < files.Length; i++)
            {
                Dictionary<string, List<(double flight, double dwell)>> probe
                    = new Dictionary<string, List<(double flight, double dwell)>>();

                string probeName = files[i].Substring(files[i].Length - 9);
                try
                {
                    using (var reader = new StreamReader(files[i]))
                    using (var csv = new CsvReader(reader, config))
                    {
                        if (probeName == testDataFileName)
                        {
                            continue;
                        }


                        try
                        {
                            sfm[i] = csv.GetRecords<SingleRowModel>().ToList();
                        }
                        catch (Exception exception)
                        {
                            sfm[i] = new List<SingleRowModel>();
                            System.Windows.MessageBox.Show($"{files[i]} cannot be converted");
                            continue;
                        }
                    }
                }
                catch (IOException exception)
                {
                    System.Windows.MessageBox.Show($"{files[i]} is being used by another process!!!");
                    continue;
                }

                // setup and add category
                if (sfm[i].Count > 0)
                {
                    for (int j = 0; j< sfm[i].Count; j++)
                    {
                        
                        string strKey = sfm[i][j].KeyName;
                        if (probe.ContainsKey(strKey))
                        {
                            probe[strKey].Add((sfm[i][j].TimeFromPrev, sfm[i][j].TimePressed));
                        }
                        else
                        {
                            probe[strKey] = new List<(double flight, double dwell)>()
                            {
                                (sfm[i][j].TimeFromPrev, sfm[i][j].TimePressed)
                            };
                        }
                    }
                }

                TrainSample? ts = this.AddTrainSample(probe, probeName);
                if (ts != null) trainSamples.Add(ts);

            }
        }

       

    }
}
