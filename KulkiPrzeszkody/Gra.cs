using System;
using System.Collections.Generic;
using System.Drawing;

namespace kulki
{
    internal class Gra
    {
        public List<Kulka> Kulki { get; private set; }
        public List<Przeszkoda> Przeszkody { get; private set; }
        private int przesunięciePrzeszkód { get; set; }
        public bool WszystkieKulkiSpadły { get; private set; }

        public Gra(int przesunięciePrzeszkód)
        {
            Kulki = new List<Kulka>();
            Przeszkody = new List<Przeszkoda>();
            this.przesunięciePrzeszkód = przesunięciePrzeszkód;
        }

        public void ResetKulkiSpadły()
        {
            WszystkieKulkiSpadły = false;
        }

        public void DodajKulkę(Kulka kulka)
        {
            Kulki.Add(kulka);
        }

        public void DodajPrzeszkodę(Przeszkoda przeszkoda)
        {
            Przeszkody.Add(przeszkoda);
        }

        public void UsuńKulkę(Kulka kulka)
        {
            Kulki.Remove(kulka);
        }

        public void UsuńPrzeszkodę(Przeszkoda przeszkoda)
        {
            Przeszkody.Remove(przeszkoda);
        }

        public void Reset()
        {
            Kulki.Clear();
            Przeszkody.Clear();
            ResetKulkiSpadły();
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
            bool wszystkiePrzeszkodyUsunięte = Przeszkody.Count == 0 || Przeszkody.TrueForAll(p => p is PrzeszkodaGranica);
            bool przeszkodaDotknęłaDolnejGranicy = Przeszkody.Any(p => p.Obszar.Bottom >= dolnaGranicaY && !(p is PrzeszkodaGranica));

            return wszystkiePrzeszkodyUsunięte || przeszkodaDotknęłaDolnejGranicy;
        }

        private void SprawdźPrzesunięcie()
        {
            if (Kulki.Count == 0 && !WszystkieKulkiSpadły)
            {
                WszystkieKulkiSpadły = true;
                PrzesuńPrzeszkodyWDół();
            }
        }

        private void UsuńKulki(List<Kulka> kulkiDoUsunięcia)
        {
            foreach (var kulka in kulkiDoUsunięcia)
            {
                UsuńKulkę(kulka);
            }
        }

        private void UsuńPrzeszkody(List<Przeszkoda> przeszkodyDoUsunięcia)
        {
            foreach (var przeszkoda in przeszkodyDoUsunięcia)
            {
                UsuńPrzeszkodę(przeszkoda);
            }
        }

        public void SymulujKrok()
        {
            List<Kulka> kulkiDoUsunięcia = new List<Kulka>();
            List<Przeszkoda> przeszkodyDoUsunięcia = new List<Przeszkoda>();

            foreach (var kulka in Kulki)
            {
                kulka.AktualizujPozycję();
                foreach (var przeszkoda in Przeszkody)
                {
                    if (przeszkoda.CzyKolizja(kulka))
                    {
                        przeszkoda.SprawdźKolizję(kulka);
                        if (przeszkoda.DoUsunięcia)
                            przeszkodyDoUsunięcia.Add(przeszkoda);
                    }
                }
                if (kulka.DoUsunięcia)
                    kulkiDoUsunięcia.Add(kulka);
            }

            UsuńKulki(kulkiDoUsunięcia);
            UsuńPrzeszkody(przeszkodyDoUsunięcia);

            SprawdźPrzesunięcie();
        }
    }
}
