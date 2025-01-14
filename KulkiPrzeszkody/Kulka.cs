using System;
using System.Drawing;

namespace kulki
{
    internal class Kulka
    {
        public PointF Pozycja { get; set; }
        public PointF Prędkość { get; set; } //prędkość jako wektor 2D
        public SizeF Rozmiar { get; set; } //ważne: rozmiar ustawiony z Form1 na 10, więc kulka mieści się pomiędzy przeszkodami, co czasem widać, ale to świadoma decyzja, żeby gra była mniej przewidywalna
        public bool DoUsunięcia { get; private set; }

        public Kulka(PointF startPozycja, PointF startPrędkość, SizeF rozmiar)
        {
            Pozycja = startPozycja;
            Prędkość = startPrędkość;
            Rozmiar = rozmiar;
            DoUsunięcia = false;
        }

        public void OznaczDoUsunięcia()
        {
            DoUsunięcia = true;
        }

        public void AktualizujPozycję() //ruch w osi X, Y
        {
            Pozycja = new PointF(Pozycja.X + Prędkość.X, Pozycja.Y + Prędkość.Y);
        }

        public void ZmieńKierunek(float nowaPrędkośćX, float nowaPrędkośćY) //przy odbiciach
        {
            Prędkość = new PointF(nowaPrędkośćX, nowaPrędkośćY);
        }
    }
}
