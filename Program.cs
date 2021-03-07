using System;

namespace LogFinder
{
    class Program
    {
        static void Main(string[] args)
        {
            string _filename = "log.txt";
            //string _filename = @"E:\oo\test.txt";

            DateTime first = new DateTime(2016,01,05); 
            DateTime second = new DateTime(2017,03,28);

            Services services = new Services();

            services.LoadLog(_filename);

            var results = services.Search(first,second);

        }
    }
}
