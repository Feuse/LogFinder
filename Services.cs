using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

namespace LogFinder
{
    public class DataHolder
    {
        public bool StartFound { get; set; }
        public bool EndFound { get; set; }
        public DateTime StartDateText { get; set; }
        public DateTime PivotText { get; set; }
        public DateTime EndDateText { get; set; }
        public long LengthIndex { get; set; }
        public long PivotLengthIndex { get; set; }
        public DateTime SearchPivotText { get; set; }//search
        public DateTime SearchStartText { get; set; }//search
        public DateTime SearchEndText { get; set; } //search
        public bool Flag { get; set; }
        public int OverFlow { get; set; }
        public long StartSearchtPivotIndex { get; set; } //search
        public long EndSearchtPivotIndex { get; set; }//search
        public long StartDateIndex { get; set; }
        public long EndDateIndex { get; set; }
        public long FoundEndPosition { get; set; }
        public long FoundStartPosition { get; set; }
        public string Data { get; set; }
    }
    public class Services
    {
        private string filename;
        public void LoadLog(string _filename)
        {
            filename = _filename;
        }

        public string Search(DateTime startSearch, DateTime endSearch)
        {
            DataHolder dh = new DataHolder();
            dh.SearchStartText = DateTime.Parse(startSearch.Date.ToString("yyyy-MM-dd"));
            dh.SearchEndText = DateTime.Parse(endSearch.Date.ToString("yyyy-MM-dd"));
            var last = ReadLastLine(filename);
            var first = ReadFirstLine(filename);

            dh.StartDateText = DateTime.Parse(first.Substring(0, 10));
            dh.EndDateText = DateTime.Parse(last.Substring(0, 10));
            ReadMiddleLine(filename, dh);

            TimeSpan ts = endSearch.Subtract(startSearch);
            dh.SearchPivotText = DateTime.Parse(startSearch.AddMinutes(ts.TotalMinutes / 2).Date.ToString("yyyy-MM-dd"));

           return FindRange(dh);

        }
        public string FindRange(DataHolder dh)
        {
            if (!(dh.StartFound && dh.EndFound))
            {
                if (dh.SearchPivotText > dh.PivotText)
                {
                    var nh = Search(filename, dh);
                    return FindRange(nh);
                }
                else if (dh.SearchPivotText < dh.PivotText)
                {
                    var nh = Search(filename, dh);
                    return FindRange(nh);
                }
            }
            return dh.Data;
        }

        private DataHolder ReadMiddleLine(string filename, DataHolder dh)
        {
            using (System.IO.Stream fs = System.IO.File.OpenRead(filename))
            {
                if (fs.Length == 0)
                {
                    return null;
                }
                int byteFromFile = 0;

                if (dh.PivotLengthIndex == 0)
                {
                    fs.Position = fs.Length / 2;
                    dh.LengthIndex = fs.Length;

                    byteFromFile = fs.ReadByte();

                    while (byteFromFile != '\n')
                    {
                        byteFromFile = fs.ReadByte();
                    }

                    byte[] bytes = new System.IO.BinaryReader(fs).ReadBytes(10);
                    dh.PivotLengthIndex = fs.Position;
                    fs.Close();
                    dh.PivotText = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));
                }
                return dh;
            }
        }
        public DataHolder Search(string filename, DataHolder dh)
        {

            using (System.IO.Stream fs = System.IO.File.OpenRead(filename))
            {
                int byteFromFile = 0;
                if (dh.SearchPivotText > dh.PivotText)
                {
                    dh.StartDateText = dh.PivotText;
                    dh.EndDateIndex = fs.Length;
                    dh.StartDateIndex = dh.PivotLengthIndex;
                    dh.PivotLengthIndex = (dh.EndDateIndex + dh.StartDateIndex) / 2;

                    //find center date
                    fs.Position = dh.PivotLengthIndex;

                    byteFromFile = fs.ReadByte();
                    while (byteFromFile != '\n')
                    {
                        byteFromFile = fs.ReadByte();
                    }

                    byte[] bytes = new System.IO.BinaryReader(fs).ReadBytes(11);

                    dh.PivotText = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));

                    //end input overflow pivot, find end input from Pivot index.
                    if (dh.SearchEndText > dh.PivotText) 
                    {
                        fs.Position = dh.PivotLengthIndex;
                        var dataCheck = dh.PivotText;
                        var tempPos = dh.EndDateIndex;
                        while (dataCheck != dh.SearchEndText && dataCheck < dh.SearchEndText)
                        {
                            tempPos = fs.Length;
                            byteFromFile = fs.ReadByte();

                            while (byteFromFile != '\n')
                            {
                                byteFromFile = fs.ReadByte();
                            }
                            bytes = new System.IO.BinaryReader(fs).ReadBytes(11);
                            var res = System.Text.Encoding.UTF8.GetString(bytes);
                            if (res == "")
                            {
                             
                                dh.FoundEndPosition= fs.Position - 11;
                                dh.EndFound = true;
                                dh.SearchEndText = dataCheck;
                                break;
                            }
                            dataCheck = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));

                        }

                        while (dataCheck == dh.SearchEndText)
                        {
                            fs.Position = tempPos;

                            while (byteFromFile != '\n')
                            {
                                byteFromFile = fs.ReadByte();
                            }
                            bytes = new System.IO.BinaryReader(fs).ReadBytes(11);
                            var res = System.Text.Encoding.UTF8.GetString(bytes);
                            if (res == "")
                            {
                                break;
                            }
                            dataCheck = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));
                        }
                        if (!dh.EndFound)
                        {

                        if (dataCheck < dh.SearchEndText)
                        {
                            
                            dh.FoundEndPosition = fs.Position;
                            dh.EndFound = true;
                        }
                        else
                        {
                            dh.FoundEndPosition = fs.Position - 11;
                            dh.EndFound = true;
                        }
                        }
                    }
                    //start input overflow pivot, find start input 
                    if (dh.StartDateText > dh.SearchStartText)
                    {
                        fs.Position = dh.StartDateIndex;
                        var dateCheck = dh.SearchStartText;
                        var currData = dh.StartDateText;

                        while (currData != dateCheck)
                        {
                            fs.Position--;
                            fs.Position -= 12;

                            byteFromFile = fs.ReadByte();

                            while (byteFromFile != '\n')
                            {
                                fs.Position--;
                                byteFromFile = fs.ReadByte();
                                fs.Position--;
                            }

                            if (fs.Position == 0)
                            {
                                fs.Position++;
                                fs.Position++;
                                fs.Position++;
                                var bytese = new System.IO.BinaryReader(fs).ReadBytes(11);
                                var res = System.Text.Encoding.UTF8.GetString(bytese);
                                currData = DateTime.Parse(res);
                                dh.FoundStartPosition = fs.Position - 11;
                                dh.StartFound = true;
                                break;
                            }
                            else
                            {
                                bytes = new System.IO.BinaryReader(fs).ReadBytes(11);

                                currData = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));
                                if (currData < dateCheck) // doesnt exist (passe the wanted intput)
                                {
                                    dh.FoundStartPosition = fs.Position - 11;
                                    dh.StartFound = true;
                                    break;
                                }
                            }
                        }
                        while (currData == dateCheck)
                        {
                            fs.Position--;
                            fs.Position -= 12;

                            byteFromFile = fs.ReadByte();

                            //fs.Position -= 2;
                            if (fs.Position == 0)
                            {
                                fs.Position++;
                                fs.Position++;
                                fs.Position++;
                                var bytese = new System.IO.BinaryReader(fs).ReadBytes(11);
                                dh.FoundStartPosition = fs.Position - 11;
                                dh.StartFound = true;
                                break;
                            }
                            else
                            {
                                while (byteFromFile != '\n' && fs.Position != 0)
                                {

                                    fs.Position--;
                                    byteFromFile = fs.ReadByte();
                                    fs.Position--;
                                }
                                bytes = new System.IO.BinaryReader(fs).ReadBytes(11);
                                currData = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));
                                if (currData == dateCheck)
                                {
                                    dh.StartFound = true;
                                    dh.FoundStartPosition = fs.Position - 11;
                                }
                            }
                        }

                    }
                    if (dh.EndFound && dh.StartFound)
                    {
                        byte[] found = new byte[dh.FoundEndPosition - dh.FoundStartPosition];
                        fs.Position = dh.FoundStartPosition;
                        fs.Read(found, 0, (int)(dh.FoundEndPosition - dh.FoundStartPosition));

                        string result = System.Text.Encoding.UTF8.GetString(found);
                        dh.Data = result;
                    }
                    fs.Close();


                    return dh;
                }
                else if (dh.SearchPivotText < dh.PivotText)
                {
                    dh.EndDateText = dh.PivotText;
                    dh.EndDateIndex = dh.PivotLengthIndex;
                    dh.PivotLengthIndex = (dh.EndDateIndex + dh.StartDateIndex) / 2;

                    fs.Position = dh.StartDateIndex;

                    byteFromFile = fs.ReadByte();
                    while (byteFromFile != '\n')
                    {
                        byteFromFile = fs.ReadByte();
                    }

                    var bytes = new System.IO.BinaryReader(fs).ReadBytes(11);
                    dh.StartDateText = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));
                    fs.Position = dh.PivotLengthIndex;

                    byteFromFile = fs.ReadByte();
                    while (byteFromFile != '\n')
                    {
                        byteFromFile = fs.ReadByte();
                    }
                    bytes = new System.IO.BinaryReader(fs).ReadBytes(11);

                    dh.PivotText = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));

                    // Is the searching input bigger than the mid point ( over flow), if so, start reading until you hit it.
                    if (dh.SearchEndText > dh.PivotText) 
                    {
                        fs.Position = dh.PivotLengthIndex;
                        var dataCheck = dh.PivotText;
                        var tempData = dataCheck;
                        while (dataCheck != dh.SearchEndText && dataCheck < dh.SearchEndText)
                        {
                            byteFromFile = fs.ReadByte();

                            while (byteFromFile != '\n')
                            {
                                byteFromFile = fs.ReadByte();
                            }
                            bytes = new System.IO.BinaryReader(fs).ReadBytes(11);
                            dataCheck = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));
                        }

                        while (dataCheck == dh.SearchEndText)
                        {
                            byteFromFile = fs.ReadByte();

                            while (byteFromFile != '\n')
                            {
                                byteFromFile = fs.ReadByte();
                            }
                            bytes = new System.IO.BinaryReader(fs).ReadBytes(11);
                            dataCheck = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));

                        }
                        if (dataCheck < dh.SearchEndText)
                        {
                            dh.FoundEndPosition = fs.Position;
                            dh.EndFound = true;
                        }
                        else
                        {
                            dh.FoundEndPosition = fs.Position - 11;
                            dh.EndFound = true;
                        }
                    }
                    //start input overflow pivot, find start input 
                    if (dh.StartDateText > dh.SearchStartText && !dh.StartFound)
                    {
                            fs.Position = dh.StartDateIndex;
                        if (fs.Position == 0)
                        {
                            fs.Position++;
                            fs.Position++;
                            fs.Position++;
                            var bytese = new System.IO.BinaryReader(fs).ReadBytes(11);
                            var tt = System.Text.Encoding.UTF8.GetString(bytese);

                            dh.FoundStartPosition = fs.Position - 11;
                            dh.StartFound = true;
                        }
                        else
                        {
                            var dateCheck = dh.SearchStartText;
                            var currData = dh.StartDateText;
                            //    fs.Position = dh.StartSearchtPivotIndex;
                            while (currData != dateCheck)
                            {
                                fs.Position--;
                                fs.Position -= 12;

                                byteFromFile = fs.ReadByte();

                                while (byteFromFile != '\n' && fs.Position != 0)
                                {
                                    fs.Position--;
                                    byteFromFile = fs.ReadByte();
                                    fs.Position--;
                                }
                                if (fs.Position == 0)
                                {
                                    fs.Position++;
                                    fs.Position++;
                                    fs.Position++;
                                    var bytese = new System.IO.BinaryReader(fs).ReadBytes(11);
                                    var tt = System.Text.Encoding.UTF8.GetString(bytese);
                                    currData = DateTime.Parse(tt);
                                    dh.FoundStartPosition = fs.Position - 11;
                                    dh.StartFound = true;
                                    break;
                                }
                                else
                                {
                                    bytes = new System.IO.BinaryReader(fs).ReadBytes(11);
                                    currData = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));
                                }
                            }

                            while (currData == dateCheck)
                            {
                                fs.Position--;
                                fs.Position -= 12;

                                byteFromFile = fs.ReadByte();

                                if (fs.Position == 0)
                                {
                                    fs.Position++;
                                    fs.Position++;
                                    fs.Position++;

                                    dh.FoundStartPosition = fs.Position - 11;
                                    dh.StartFound = true;
                                    break;
                                }
                                else
                                {
                                    while (byteFromFile != '\n' && fs.Position != 0)
                                    {
                                        fs.Position--;
                                        byteFromFile = fs.ReadByte();
                                        fs.Position--;
                                    }
                                    bytes = new System.IO.BinaryReader(fs).ReadBytes(11);
                                    currData = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));
                                    if (currData == dateCheck)
                                    {
                                        dh.StartFound = true;
                                        dh.FoundStartPosition = fs.Position - 11;
                                    }
                                }
                            }
                        }

                    }
                    if (dh.StartDateText == dh.SearchStartText)
                    {
                        dh.FoundStartPosition = dh.StartDateIndex;
                        dh.StartFound = true;
                    }
                    if (dh.EndDateText == dh.SearchEndText)
                    {
                        var dataCheck = dh.PivotText;
                        while (dataCheck == dh.SearchEndText)
                        {
                            byteFromFile = fs.ReadByte();

                            while (byteFromFile != '\n')
                            {
                                byteFromFile = fs.ReadByte();
                            }
                            bytes = new System.IO.BinaryReader(fs).ReadBytes(11);
                            dataCheck = DateTime.Parse(System.Text.Encoding.UTF8.GetString(bytes));

                        }
                        if (dataCheck < dh.SearchEndText)
                        {
                            dh.FoundEndPosition = fs.Position-11;
                            dh.EndFound = true;
                        }
                        else
                        {
                            dh.FoundEndPosition = fs.Position ;
                            dh.EndFound = true;
                        }

                        dh.EndFound = true;
                    }
                    if (dh.StartFound && dh.EndFound)
                    {
                        byte[] found = new byte[dh.FoundEndPosition - dh.FoundStartPosition];
                        fs.Position = dh.FoundStartPosition;
                        fs.Read(found, 0, (int)(dh.FoundEndPosition - dh.FoundStartPosition));

                        string result = System.Text.Encoding.UTF8.GetString(found);
                        dh.Data = result;
                    }
                    fs.Close();

                    return dh;
                }
                else
                {
                    return dh;
                }
            }

        }

        public string ReadFirstLine(string path)
        {
            using (System.IO.Stream fs = System.IO.File.OpenRead(path))
            {
                if (fs.Length == 0)
                {
                    return null;
                }
                fs.Position++;
                fs.Position++;
                fs.Position++;
                byte[] bytes = new System.IO.BinaryReader(fs).ReadBytes(11);
                fs.Close();

                return System.Text.Encoding.UTF8.GetString(bytes);

            }
        }
        public string ReadLastLine(string path)
        {
            using (System.IO.Stream fs = System.IO.File.OpenRead(path))
            {
                if (fs.Length == 0)
                {
                    return null;
                }
                fs.Position = fs.Length - 11;
                int byteFromFile = fs.ReadByte();
                while (byteFromFile != '\n' && fs.Position != 0)
                {
                    fs.Position--;
                    byteFromFile = fs.ReadByte();
                    fs.Position--;
                }
                var bytes = new System.IO.BinaryReader(fs).ReadBytes(11);

                return System.Text.Encoding.UTF8.GetString(bytes);
            }
        }
    }
}
