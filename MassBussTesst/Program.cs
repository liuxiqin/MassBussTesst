using System;

namespace MassBussTesst
{
    class Program
    {
        private static void Main(string[] args)
        {
            var suite = new SzynaTests();
            RunTest(suite, o => o.WysłanyKomunikatDocieraDoCelu());
            RunTest(suite, o => o.PozwalaWysyłaćSekwencyjnie());
            RunTest(suite, o => o.WysyłanieJestTransakcyjne_PoRollbackuKomunikatNieDociera());
            RunTest(suite, o => o.WPrzypadkuBłęduPrzyOdbieraniuKomunikatWracaIJestPonawiany());
            //RunTest(suite, o => o.WPrzypadkuPonawianiaKolejnośćKomunikatówNieJestZachowana());
            RunTest(suite, o => o.MożnaWymusićSekwencyjnośćNawetWPrzypadkuPonawiania());
        }

        private static void RunTest(SzynaTests suite, Action<SzynaTests> test)
        {
            try
            {
                suite.SetUpEachTest();
                test(suite);
            }
            catch (Exception)
            {
                suite.TearDownEachTest();
                throw;
            }
        }
    }
}