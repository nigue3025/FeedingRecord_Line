using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JerryRecord
{
    public class DaySplitter
    {
        string Filename;
        List<string> LoadedFIleLineLst = new List<string>();
        public List<string> EachDayRecord = new List<string>();
        public DaySplitter(string filename)
        {
            Filename = filename;
        }
        public DaySplitter()
        {

        }
        public void LoadFileToList()
        {
            LoadedFIleLineLst.Clear();
            using (System.IO.StreamReader sr = new System.IO.StreamReader(Filename))
            {
                string str;
                while ((str = sr.ReadLine()) != null)
                {
                    LoadedFIleLineLst.Add(str);
                }

            }
        }



        public void generateDayRecord()
        {
            List<string> DateLst = new List<string>();

            List<string> EachDayTotalMilk = new List<string>();
            StringBuilder sb = new StringBuilder();
            List<string> OutDetailRecord = new List<string>();
            List<string> OutRecordStat = new List<string>();
            foreach (var strln in LoadedFIleLineLst)
            {

                var strAry = strln.Split(':');

                //if(strAry.Length==1)
                if (!strln.Contains(":"))
                {
                    if (sb.ToString().Length > 0)
                        EachDayRecord.Add(sb.ToString());
                    sb.Clear();
                    sb.Append(strln);
                    sb.Append("|");
                    continue;

                }

                sb.Append(strln);
                sb.Append("|");
            }

            EachDayRecord.Add(sb.ToString());
            sb.Clear();
        }

        public void run()
        {
            LoadFileToList();
            generateDayRecord();
        }
    }

    public class MilkRecorder
    {
        char[] foodsplitChar = new char[] { ',', '，', '?' };
        List<string> OutDetailRecord = new List<string>();
        List<string> OutRecordStat = new List<string>();

        public MilkRecorder()
        {

        }
        
        public void writeStatFile(string filename= "統計資訊.csv")
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filename, false, Encoding.UTF8))
            {
                string statLable = string.Format("日期,母,配,母+配,進食次數\n");
                sw.Write(statLable);
                sw.Write(string.Join("\n", OutRecordStat));
            }
        }

        public void writeCollectedFile(string filename= "一般資訊.csv")
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filename, false, Encoding.UTF8))
            {
                sw.Write(string.Join("\n", OutDetailRecord));
            }
        }

        public void run(DaySplitter DailyData)
        {
            var EachDayRecord = DailyData.EachDayRecord;
            List<string> DateLst = new List<string>();
            //List<string> EachDayRecord = new List<string>();
            List<string> EachDayTotalMilk = new List<string>();
            StringBuilder sb = new StringBuilder();

            OutDetailRecord.Clear();
            OutRecordStat.Clear();


            StringBuilder daySb = new StringBuilder();
            StringBuilder dayStatSb = new StringBuilder();
            foreach (var aRecord in EachDayRecord)
            {
                daySb.Clear();
                dayStatSb.Clear();
                var recordLst = aRecord.Split('|').ToList();
                var dateStr = recordLst[0];
                recordLst.RemoveAt(0);
                DateLst.Add(dateStr);

                daySb.Append(dateStr);
                daySb.Append(",");

                dayStatSb.Append(dateStr);
                dayStatSb.Append(",");

                var foodLst = recordLst.Where(a => a.Contains("配") || a.Contains("母")).Select(a => a).ToList();

                int totalMMilk = 0;
                int totalFMilk = 0;


                foreach (var fooddata in foodLst)
                {
                    var foodary = fooddata.Split(foodsplitChar).ToList();
                    
                    if (foodary.Count > 1)
                    {
                        var time = foodary[0].Split(' ').Last();
                        var foodDetail = foodary[1];
                        string MMilk = "0";
                        string FMilk = "0";
                        var AllMilk = foodDetail.Split(new char[] { '母', '配' });

                        if (AllMilk.Length > 2)
                        {
                            MMilk = AllMilk[0];
                            FMilk = AllMilk[1];
                        }
                        else
                        {
                            if (fooddata.Contains("母"))
                            {
                                MMilk = AllMilk[0];
                                FMilk = "0";
                            }
                            else
                            {
                                MMilk = "0";
                                FMilk = AllMilk[0];
                            }


                        }
                        string aFeedingRecord = string.Format("時間:{0}_母:{1}_配:{2}", time, MMilk, FMilk);
                        daySb.Append(aFeedingRecord);
                        daySb.Append(",");
                        totalMMilk += int.Parse(MMilk);
                        totalFMilk += int.Parse(FMilk);



                    }


                }
                OutDetailRecord.Add(daySb.ToString());


                EachDayTotalMilk.Add(string.Format("{0},{1},{2},{3}", totalMMilk, totalFMilk, totalFMilk + totalMMilk, foodLst.Count));
                dayStatSb.Append(EachDayTotalMilk.Last());
                OutRecordStat.Add(dayStatSb.ToString());
            }
            
        }

    }

    class Program
    {

        static void Main(string[] args)
        {
            char[] foodsplitChar = new char[] { ',', '，', '?' };


            DaySplitter daySplitter = new DaySplitter("BbRecord.txt");
            daySplitter.run();
            var EachDayRecord = daySplitter.EachDayRecord;

            MilkRecorder milkRecorder = new MilkRecorder();
            milkRecorder.run(daySplitter);
            milkRecorder.writeCollectedFile("Report/一般資訊.csv");
            milkRecorder.writeStatFile("Report/統計資訊.csv");
            

        }
    }
}
