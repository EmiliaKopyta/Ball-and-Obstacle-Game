using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kulki
{
    internal class GraMotor
    {
        public List<Kulka> Kulki { get; private set; }
        public List<Przeszkoda> Przeszkody { get; private set; }

        private PriorityQueue<Wydarzenie, double> kalendarzWydarzeń;
        public double CzasSymulacji { get; private set; }
        private int przesunięciePrzeszkód { get; set; }
        public bool WszystkieKulkiSpadły { get; private set; }


        //żeby jakoś się odwołać do metody z Form1
        public delegate void AktualizujWizualizacjeDelegate();
        public AktualizujWizualizacjeDelegate AktualizujWizualizacjeCallback { get; set; }

        public GraMotor(int przesunięciePrzeszkód)
        {
            Kulki = new List<Kulka>();
            Przeszkody = new List<Przeszkoda>();
            kalendarzWydarzeń = new PriorityQueue<Wydarzenie, double>();
            CzasSymulacji = 0;
            this.przesunięciePrzeszkód = przesunięciePrzeszkód;
        }

        public List<Kulka> PobierzKulki()
        {
            return Kulki;
        }

        public void ResetKulkiSpadły()
        {
            WszystkieKulkiSpadły = false;
        }

        public void DodajWydarzenie(Wydarzenie wydarzenie)
        {
            kalendarzWydarzeń.Enqueue(wydarzenie, wydarzenie.Czas - CzasSymulacji);
        }

        public async Task RzućKulki(PointF kliknietaPozycja, PointF punktStartowyKulka, float predkoscKulki, int opóźnienieRzutu)
        {
            PointF kierunek = new PointF(kliknietaPozycja.X - punktStartowyKulka.X, kliknietaPozycja.Y - punktStartowyKulka.Y);

            //normalizacja wektora kierunku (bo uzależniam się od miejsca kliknięcia myszką)
            float dlugosc = (float)Math.Sqrt(kierunek.X * kierunek.X + kierunek.Y * kierunek.Y);
            kierunek = new PointF(kierunek.X / dlugosc * predkoscKulki, kierunek.Y / dlugosc * predkoscKulki);

            var kulkiDoRzutu = Kulki.ToList();

            foreach (var kulka in kulkiDoRzutu)
            {
                if (Kulki.Contains(kulka))
                {
                    kulka.ZmieńKierunek(kierunek.X, kierunek.Y);
                    await Task.Delay(opóźnienieRzutu); //opóźnienie między rzutami kulek
                }
            }
        }

        public bool CzyKoniecGry(float dolnaGranicaY)
        {
            bool wszystkiePrzeszkodyUsuniete = Przeszkody.Count == 0 || Przeszkody.TrueForAll(p => p is PrzeszkodaGranica);
            bool przeszkodaDotknelaDolnejGranicy = Przeszkody.Any(p => p.Obszar.Bottom >= dolnaGranicaY && !(p is PrzeszkodaGranica));

            return wszystkiePrzeszkodyUsuniete || przeszkodaDotknelaDolnejGranicy;
        }

        //jeśli przeszkoda/kulka jest związana z UI (PictureBox), trzeba ją też tam dodać/usunąć Controls.Remove...
        public void DodajKulkę(Kulka kulka)
        {
            Kulki.Add(kulka);
            AktualizujZdarzenia();
        }

        public void UsuńKulkę(Kulka kulka)
        {
            Kulki.Remove(kulka);
            AktualizujZdarzenia();
        }

        public void DodajPrzeszkodę(Przeszkoda przeszkoda)
        {
            Przeszkody.Add(przeszkoda);
            AktualizujZdarzenia();
        }

        public void UsuńPrzeszkodę(Przeszkoda przeszkoda)
        {
            Przeszkody.Remove(przeszkoda);
            AktualizujZdarzenia();
        }

        private double ObliczCzasDoKolizji(Kulka kulka, Przeszkoda przeszkoda)
        {
            //na podstawie równania ruchu jednostajnego dla krawędzi przekształconego na czas
            //wzdłuż osi OX: t_krawędź=(x_krawędź-x_0)/v_x
            //wzdłuż osi OY: t_krawędź = (y_krawędź - y_0) / v_y

            //do tego sprawdzenie, czy dla obliczonych czasów kulka znajdzie się w granicach przeszkody w drugim wymiarze i z tych czasów minimalny

            double czasKolizji = double.PositiveInfinity;

            //z lewą i prawą krawędzią, wzdłuż osi OX
            if (kulka.Prędkość.X != 0)
            {
                double czasLewa = (przeszkoda.Obszar.Left - kulka.Pozycja.X) / kulka.Prędkość.X;
                double czasPrawa = (przeszkoda.Obszar.Right - kulka.Pozycja.X) / kulka.Prędkość.X;

                if (czasLewa > 0 && kulka.Pozycja.Y + czasLewa * kulka.Prędkość.Y >= przeszkoda.Obszar.Top &&
                    kulka.Pozycja.Y + czasLewa * kulka.Prędkość.Y <= przeszkoda.Obszar.Bottom)
                        czasKolizji = Math.Min(czasKolizji, czasLewa);

                if (czasPrawa > 0 && kulka.Pozycja.Y + czasPrawa * kulka.Prędkość.Y >= przeszkoda.Obszar.Top &&
                    kulka.Pozycja.Y + czasPrawa * kulka.Prędkość.Y <= przeszkoda.Obszar.Bottom)
                        czasKolizji = Math.Min(czasKolizji, czasPrawa);
            }

            //z górną i dolną krawędzią, wzdłuż osi OY
            if (kulka.Prędkość.Y != 0)
            {
                double czasGórna = (przeszkoda.Obszar.Top - kulka.Pozycja.Y) / kulka.Prędkość.Y;
                double czasDolna = (przeszkoda.Obszar.Bottom - kulka.Pozycja.Y) / kulka.Prędkość.Y;

                if (czasGórna > 0 && kulka.Pozycja.X + czasGórna * kulka.Prędkość.X >= przeszkoda.Obszar.Left &&
                    kulka.Pozycja.X + czasGórna * kulka.Prędkość.X <= przeszkoda.Obszar.Right)
                        czasKolizji = Math.Min(czasKolizji, czasGórna);

                if (czasDolna > 0 && kulka.Pozycja.X + czasDolna * kulka.Prędkość.X >= przeszkoda.Obszar.Left &&
                    kulka.Pozycja.X + czasDolna * kulka.Prędkość.X <= przeszkoda.Obszar.Right)
                        czasKolizji = Math.Min(czasKolizji, czasDolna);
            }
            return czasKolizji > 0 ? czasKolizji : double.PositiveInfinity;
        }


        public void AktualizujZdarzenia()
        {
            kalendarzWydarzeń.Clear();

            foreach (var kulka in Kulki)
            {
                foreach (var przeszkoda in Przeszkody)
                {
                    double czasDoKolizji = ObliczCzasDoKolizji(kulka, przeszkoda);
                    if (czasDoKolizji < double.PositiveInfinity) //chodzi o to czy kolizja nastąpi
                    {
                        var wydarzenie = new Wydarzenie(CzasSymulacji + czasDoKolizji, kulka, przeszkoda);
                        kalendarzWydarzeń.Enqueue(wydarzenie, czasDoKolizji);
                    }
                }
            }
        }

        public void PrzesuńPrzeszkodyWDół()
        {
            foreach (var przeszkoda in Przeszkody)
            {
                if (!(przeszkoda is PrzeszkodaGranica))
                {
                    var nowyObszar = new RectangleF(przeszkoda.Obszar.X, przeszkoda.Obszar.Y + przesunięciePrzeszkód, przeszkoda.Obszar.Width, przeszkoda.Obszar.Height);
                    przeszkoda.ZaktualizujObszar(nowyObszar);
                }
            }
        }

        private void SprawdźPrzesunięcie()
        {
            if (Kulki.Count == 0 && !WszystkieKulkiSpadły)
            {
                WszystkieKulkiSpadły = true;
                PrzesuńPrzeszkodyWDół();
            }
        }

        public async Task SymulujKrok()
        {
            if (kalendarzWydarzeń.Count > 0)
            {
                var najblizszeWydarzenie = kalendarzWydarzeń.Peek();
                double czasDoWydarzenia = najblizszeWydarzenie.Czas - CzasSymulacji;

                if (czasDoWydarzenia > 0)
                    await Task.Delay((int)(czasDoWydarzenia * 1000)); //sekundy na milisekundy

                CzasSymulacji = najblizszeWydarzenie.Czas;

                najblizszeWydarzenie = kalendarzWydarzeń.Dequeue();
                najblizszeWydarzenie.Wykonaj();

                Przeszkody.RemoveAll(p => p.DoUsunięcia);
                Kulki.RemoveAll(p => p.DoUsunięcia);
                SprawdźPrzesunięcie();

                AktualizujZdarzenia();
                AktualizujWizualizacjeCallback?.Invoke();
            }
        }

    }
}
