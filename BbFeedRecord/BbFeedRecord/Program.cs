using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JerryRecord
{
    class Program
    {
        
        static void Main(string[] args)
        {
            char[] foodsplitChar = new char[] { ',', '，', '?' };
            List<string> strLst = new List<string>();

            using (System.IO.StreamReader sr = new System.IO.StreamReader("BbRecord.txt"))
            {
                string str;
                while((str=sr.ReadLine())!=null)
                {
                    strLst.Add(str);
                }

            }


            List<string> DateLst = new List<string>();
            List<string> EachDayRecord = new List<string>();
            List<string> EachDayTotalMilk = new List<string>();
            StringBuilder sb = new StringBuilder();
            List<string> OutDetailRecord = new List<string>();
            List<string> OutRecordStat = new List<string>();
            foreach(var strln in strLst)
            {

                var strAry = strln.Split(':');
                if (strln == "2022.08.22 星期一")
                    strAry = strAry;

                    //if(strAry.Length==1)
                if (!strln.Contains(":"))
                {
                    if(sb.ToString().Length>0)
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
            EachDayRecord = EachDayRecord;

            StringBuilder daySb = new StringBuilder();
            StringBuilder dayStatSb = new StringBuilder();
            foreach(var aRecord in EachDayRecord)
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

               var foodLst= recordLst.Where(a => a.Contains("配") || a.Contains("母")).Select(a => a).ToList();
               
                int totalMMilk = 0;
                int totalFMilk = 0;
               

                foreach (var fooddata in foodLst)
                {
                    var foodary = fooddata.Split(foodsplitChar).ToList();
                    foodary = foodary;
               
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
                        string aFeedingRecord= string.Format("時間:{0}_母:{1}_配:{2}", time, MMilk, FMilk);
                        daySb.Append(aFeedingRecord);
                        daySb.Append(",");
                        totalMMilk += int.Parse(MMilk);
                        totalFMilk += int.Parse(FMilk);



                    }


                }
                OutDetailRecord.Add(daySb.ToString());
                
                EachDayTotalMilk.Add(string.Format("母總:{0},配總:{1},共:{2},進食次數:{3}", totalMMilk, totalFMilk, totalFMilk + totalMMilk,foodLst.Count));
                dayStatSb.Append(EachDayTotalMilk.Last());
                OutRecordStat.Add(dayStatSb.ToString());
            }


            using (System.IO.StreamWriter sw = new System.IO.StreamWriter("一般資訊.csv", false, Encoding.UTF8))
            {
                sw.Write(string.Join("\n", OutDetailRecord));
            }
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter("統計資訊.csv",false,Encoding.UTF8))
            {
                sw.Write(string.Join("\n", OutRecordStat));
            }

        }
    }
}
