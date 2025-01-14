using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kulki
{
    internal class PrzeszkodaGranica : Przeszkoda
    {
        private bool czyUsuwaKulkę;
        public PrzeszkodaGranica(RectangleF obszar, bool czyUsuwaKulkę=false) //false, bo zakładam że zwykle tylko odbicie, a przeszkoda graniczna na dole usuwa
            : base(obszar, 0) //licznik tu bez znaczenia, nie usuwam granic
        {
            this.czyUsuwaKulkę = czyUsuwaKulkę;
        }
        public override void SprawdźKolizję(Kulka kulka)
        {
            if (CzyKolizja(kulka))
            {
                if (!czyUsuwaKulkę) //bardziej prawdopodobne, że nie usuwa kulki, stąd taki warunek
                {
                    Odbij(kulka);
                }
                else
                {
                    kulka.OznaczDoUsunięcia();
                }
            }
        }

    }
}
