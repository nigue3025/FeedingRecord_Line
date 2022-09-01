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
        char[] foodsplitChar = new char[] { ',', '，', '?','？' };
        List<string> OutDetailRecord = new List<string>();
        List<string> OutRecordStat = new List<string>();

        public MilkRecorder()
        {

        }
        
        public void writeStatFile(string filename= "統計資訊.csv")
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(filename, false, Encoding.UTF8))
            {
                string statLable = string.Format("日期,母,配,母+配,進食次數,平均間隔時間\n");
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

        public List<DateTime> convertToTimeList(List<string>TimeLstStr)
        {
            return TimeLstStr.Select(
              a =>
              {
                  int hr, min;
                  var timeAry = a.Split(':');
                  try
                  {
                      hr = int.Parse(timeAry[0].Substring(0, 2));
                      min = int.Parse(timeAry[1].Substring(0, 2));
                  }
                  catch
                  {
                      hr = 0;
                      min = 0;
                  }
                  return new DateTime(1, 1, 1, hr, min, 0);
              }
              ).ToList();


        }


        public double ComputeFeedTimeAvg(List<string>TimeLstStr)
        {
            var TimeLst = convertToTimeList(TimeLstStr);
            TimeLst.Sort();
            List<double> diffTimeLSt = new List<double>();
            DateTime currDt = DateTime.MinValue;
            foreach (var dt in TimeLst)
            {
                if (currDt != DateTime.MinValue)
                    diffTimeLSt.Add((dt - currDt).TotalHours);
                currDt = dt;
            }
            return diffTimeLSt.Average();
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

            //根據每日資料處理配方母奶分配(逐日處裡)
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

               
                var foodLst = recordLst.Where(a => a.Contains("配") || a.Contains("母")).Select(a => a).ToList(); // 只撈含餵奶資訊限定特定格式(e.g. 100配50母)


                #region 整理及統計配方母奶資訊
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
           
                #endregion


                #region 計算餵食間隔時間
                var foodTimeLst = foodLst.Select(a => ((a.Split(' ')[2]).Insert(2,":")).Split(foodsplitChar)[0]).ToList();
                var FeedTimeAvg=ComputeFeedTimeAvg(foodTimeLst);
               
                #endregion

                #region 產出統計報表
                dayStatSb.Append(EachDayTotalMilk.Last()+","+FeedTimeAvg.ToString("0.00"));
                OutRecordStat.Add(dayStatSb.ToString());
                #endregion  
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
