using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace kulki
{
    internal class Przeszkoda
    {
        public RectangleF Obszar { get; private set; }
        public int Licznik { get; private set; }
        public bool DoUsunięcia { get; private set; }
        public bool Nagroda{ get; private set; }
        public int Bonus { get; private set; }

        public Przeszkoda(RectangleF obszar, int licznik)
        {
            Obszar = obszar;
            Licznik = licznik;
            DoUsunięcia = false;
            Nagroda = false;
            Bonus = 0;
        }

        public void ZaktualizujObszar(RectangleF nowyObszar)
        {
            Obszar = nowyObszar;
        }

        public void UstawNagrodę(int bonus)
        {
            Nagroda = true;
            Bonus += bonus;
        }

        public void ZmniejszLicznik(int wartosc = 1)
        {
            Licznik -= wartosc;
            if (Licznik <= 0)
                DoUsunięcia = true;
        }

        public bool CzyKolizja(Kulka kulka)
        {
            return Obszar.IntersectsWith(new RectangleF(kulka.Pozycja, kulka.Rozmiar));
        }

        /*
        public void Odbij(Kulka kulka) //prosta zmiana kierunku
        { 
            if (kulka.Pozycja.X <= Obszar.Left || kulka.Pozycja.X + kulka.Rozmiar.Width >= Obszar.Right)
                kulka.ZmieńKierunek(-kulka.Prędkość.X, kulka.Prędkość.Y); //od pionowych krawędzi

            if (kulka.Pozycja.Y <= Obszar.Top || kulka.Pozycja.Y + kulka.Rozmiar.Height >= Obszar.Bottom)
                kulka.ZmieńKierunek(kulka.Prędkość.X, -kulka.Prędkość.Y); //od poziomych krawędzi
        }*/

        public void Odbij(Kulka kulka) //kąt padania = kąt odbicia, czyli zmiana odpowiedniego kierunku X lub Y na przeciwny + symetryczne odbicie względem wektora prostopadego do powierzchni odbicia
        {
            PointF wektorVKulki = kulka.Prędkość; 
            PointF wektorNormalnyPowierzchniOdbicia = new PointF(0, 0);

            if (kulka.Pozycja.X <= Obszar.Left || kulka.Pozycja.X + kulka.Rozmiar.Width >= Obszar.Right)
            {
                wektorNormalnyPowierzchniOdbicia = new PointF(1, 0); //normalna wzdłuż osi OX
            }

            if (kulka.Pozycja.Y <= Obszar.Top || kulka.Pozycja.Y + kulka.Rozmiar.Height >= Obszar.Bottom)
            {
                wektorNormalnyPowierzchniOdbicia = new PointF(0, 1); //wzdłuż osi OY
            }

            //iloczyn skalarny itd.
            float wektorPrędkościOdPowierzchni = wektorVKulki.X * wektorNormalnyPowierzchniOdbicia.X + wektorVKulki.Y * wektorNormalnyPowierzchniOdbicia.Y;
            PointF nowaPrędkość = new PointF(
                wektorVKulki.X - 2 * wektorPrędkościOdPowierzchni * wektorNormalnyPowierzchniOdbicia.X,
                wektorVKulki.Y - 2 * wektorPrędkościOdPowierzchni * wektorNormalnyPowierzchniOdbicia.Y
            );
            kulka.ZmieńKierunek(nowaPrędkość.X, nowaPrędkość.Y);
        }

        public virtual void SprawdźKolizję(Kulka kulka)
        {
            if (CzyKolizja(kulka)) 
            {
                ZmniejszLicznik();
                Odbij(kulka);
            }
        }
    }
}
