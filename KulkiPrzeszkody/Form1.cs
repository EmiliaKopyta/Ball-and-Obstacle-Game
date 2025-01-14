using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace kulki
{
    public partial class Form1 : Form
    {
        private Dictionary<Kulka, PictureBox> kulkiMapowanie; //do �ledzenia wizualizacji kulek
        private Dictionary<Przeszkoda, PictureBox> przeszkodyMapowanie; //do �ledzenia wizualizacji przeszk�d

        private int wynik = 0;
        private Gra gra;
        
        //jest tu du�o parametr�w, �eby �atwo sterowa� trudno�ci� i wygl�dem gry
        //ustawione w odpowiednich funkcjach: 
        //pozycja startowa kulek, startX i startY w inicjalizacji przeszk�d, ilo�� nowych rz�d�w tworzonych podczas gry, kolory przeszk�d/kulki

        private int szeroko��Przeszkody = 80;
        private int wysoko��Przeszkody = 35;
        private int maxLicznikPrzeszkody = 20;

        private int liczbaKolumnPrzeszk�d = 7;
        private int liczbaWierszyPrzeszk�d = 6;
        private int odst�pPrzeszkody = 10;

        private int odst�pNowePrzeszkody = 50;

        private int maxKulki = 30;
        private int rozmiarKulki = 10;
        private float predkoscKulki = 15f;

        private PointF punktStartowyKulka; //dla wszystkich kulek wsp�lny
        private int op�nienieRzutu = 200;
        private bool czyRzucanieTrwa = false;

        public Form1()
        {
            InitializeComponent();
            gra = new Gra(wysoko��Przeszkody+odst�pNowePrzeszkody); //min. o wysoko��, �eby przeszkody nie nachodzi�y na siebie i do tego dodaj� odst�p
            kulkiMapowanie = new Dictionary<Kulka, PictureBox>();
            przeszkodyMapowanie = new Dictionary<Przeszkoda, PictureBox>();
            InicjalizujGranice(10, Color.DarkBlue);
            InicjalizujPrzeszkody(szeroko��Przeszkody, wysoko��Przeszkody, liczbaKolumnPrzeszk�d, liczbaWierszyPrzeszk�d);

            punktStartowyKulka = new PointF(ClientSize.Width / 2, ClientSize.Height - 50);
            InicjalizujKulki(rozmiarKulki);
        }

        //KULKI
        private void InicjalizujKulki(int rozmiarKulki)
        {
            for (int i = 0; i < maxKulki; i++)
            {
                Kulka nowaKulka = new Kulka(punktStartowyKulka, PointF.Empty, new SizeF(rozmiarKulki, rozmiarKulki));
                gra.DodajKulk�(nowaKulka);
                PictureBox nowaKulkaPB = new PictureBox
                {
                    Width = rozmiarKulki,
                    Height = rozmiarKulki,
                    BackColor = Color.Transparent, //przezroczysty, �eby tylko elipsa by�a widoczna
                    Location = new Point((int)punktStartowyKulka.X, (int)punktStartowyKulka.Y)
                };

                nowaKulkaPB.Paint += (sender, e) => //okr�g�a kulka
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; //wyg�adzanie kraw�dzi
                    using (Brush brush = new SolidBrush(Color.Red))
                    {
                        e.Graphics.FillEllipse(brush, 0, 0, nowaKulkaPB.Width, nowaKulkaPB.Height);
                    }
                };
                Controls.Add(nowaKulkaPB);
                kulkiMapowanie[nowaKulka] = nowaKulkaPB;
            }
        }


        private async void Form1_MouseClick(object sender, MouseEventArgs e)
        {
            if (!czyRzucanieTrwa)
            {
                czyRzucanieTrwa = true;
                PointF kliknietaPozycja = new PointF(e.X, e.Y);
                await gra.Rzu�Kulki(kliknietaPozycja, punktStartowyKulka, predkoscKulki, op�nienieRzutu);

                czyRzucanieTrwa = false;
            }
        }

        //PRZESZKODY
        private Przeszkoda Utw�rzPrzeszkod�(float x, float y, int szeroko��, int wysoko��)
        {
            Random random = new Random();
            var przeszkodaProstok�tna = new RectangleF(x, y, szeroko��, wysoko��);
            var pictureBoxPrzeszkoda = new PictureBox
            {
                Width = szeroko��,
                Height = wysoko��,
                BackColor = Color.Blue,
                Location = new Point((int)przeszkodaProstok�tna.Left, (int)przeszkodaProstok�tna.Top),
                Tag = "przeszkoda"
            };

            int licznik = random.Next(1, maxLicznikPrzeszkody);
            var przeszkoda = new Przeszkoda(przeszkodaProstok�tna, licznik);

            if (licznik % 5 == 0) //przyk�adowa nagroda
            {
                przeszkoda.UstawNagrod�(10);
                pictureBoxPrzeszkoda.BackColor = Color.Yellow;
            }

            gra.DodajPrzeszkod�(przeszkoda);
            Controls.Add(pictureBoxPrzeszkoda);
            przeszkodyMapowanie[przeszkoda] = pictureBoxPrzeszkoda;

            var labelLicznik = new Label
            {
                Text = licznik.ToString(),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Width = szeroko��,
                Height = wysoko��,
                Location = new Point(0, 0)
            };

            pictureBoxPrzeszkoda.Controls.Add(labelLicznik);

            return przeszkoda;
        }

        private void AktualizujLicznikNaEtykiecie(Przeszkoda przeszkoda)
        {
            if (przeszkodyMapowanie.ContainsKey(przeszkoda))
            {
                var labelLicznik = przeszkodyMapowanie[przeszkoda].Controls.OfType<Label>().FirstOrDefault();

                if (labelLicznik != null)
                    labelLicznik.Text = przeszkoda.Licznik.ToString();
            }
        }

        private void Utw�rzRz�dPrzeszk�d(int startX, int startY, int szeroko��, int wysoko��, int liczbaKolumn)
        {
            for (int x = 0; x < liczbaKolumn; x++)
            {
                float przeszkodaX = startX + x * (szeroko�� + odst�pPrzeszkody);
                Utw�rzPrzeszkod�(przeszkodaX, startY, szeroko��, wysoko��);
            }
        }

        private void InicjalizujPrzeszkody(int szeroko��, int wysoko��, int liczbaKolumn, int liczbaWierszy)
        {
            int startX = (ClientSize.Width - (liczbaKolumn * (szeroko�� + odst�pPrzeszkody))) / 2;
            int startY = 100;

            for (int y = 0; y < liczbaWierszy; y++)
            {
                int przeszkodaY = startY + y * (wysoko�� + 10);
                Utw�rzRz�dPrzeszk�d(startX, przeszkodaY, szeroko��, wysoko��, liczbaKolumn);
            }
        }

        //GRANICE
        private void Utw�rzPrzeszkod�Granica(float x, float y, float szeroko��, float wysoko��, Color kolor, bool czyUsuwaKulk� = false)
        {
            var przeszkoda = new PrzeszkodaGranica(new RectangleF(x, y, szeroko��, wysoko��), czyUsuwaKulk�);
            var pictureBoxPrzeszkoda = new PictureBox
            {
                BackColor = kolor,
                Width = (int)szeroko��,
                Height = (int)wysoko��,
                Location = new Point((int)x, (int)y) //to potencjalnie daje b��dy zaokr�glenia, ale je akceptuj�
            };

            //podobnie wygl�da dodawanie znikaj�cych przeszk�d, ale przez r�ny typ 'przeszkoda' ci�ko wyodr�bni� uniwersaln� funkcj� do powi�zywania grafiki z logik� przeszkody bez wi�kszej komplikacji kodu
            Controls.Add(pictureBoxPrzeszkoda);
            gra.DodajPrzeszkod�(przeszkoda);
            przeszkodyMapowanie[przeszkoda] = pictureBoxPrzeszkoda;
        }

        private void InicjalizujGranice(float grubo��Granicy, Color kolor)
        {
            Utw�rzPrzeszkod�Granica(0, 0, grubo��Granicy, ClientSize.Height, kolor); //lewa
            Utw�rzPrzeszkod�Granica(ClientSize.Width - grubo��Granicy, 0, grubo��Granicy, ClientSize.Height, kolor); //prawa
            Utw�rzPrzeszkod�Granica(0, 0, ClientSize.Width, grubo��Granicy, kolor); //g�rna
            Utw�rzPrzeszkod�Granica(0, ClientSize.Height - grubo��Granicy, ClientSize.Width, grubo��Granicy, kolor, true); //dolna granica z usuwaniem
        }


        //GRA
        private void KulkaSpad�a(Kulka kulka)
        {
            if (kulkiMapowanie.ContainsKey(kulka))
            {
                Controls.Remove(kulkiMapowanie[kulka]);
                kulkiMapowanie.Remove(kulka);
            }

            if (gra.WszystkieKulkiSpad�y)
                WszystkieKulkiSpad�y();
        }

        private void WszystkieKulkiSpad�y()
        {
            gra.ResetKulkiSpad�y();
            czyRzucanieTrwa = false;
            InicjalizujKulki(rozmiarKulki);
            Przesu�PrzeszkodyWD�();
            InicjalizujPrzeszkody(szeroko��Przeszkody, wysoko��Przeszkody, liczbaKolumnPrzeszk�d, 1); //mo�na doda� wi�cej kolumn i utrudni�
        }

        private void Przesu�PrzeszkodyWD�()
        {
            foreach (var przeszkoda in gra.Przeszkody)
            {
                if (przeszkodyMapowanie.ContainsKey(przeszkoda))
                    przeszkodyMapowanie[przeszkoda].Location = new Point((int)przeszkoda.Obszar.Left, (int)przeszkoda.Obszar.Top);
            }
        }

        public void AktualizujWizualizacjeKulki()
        {
            foreach (var para in kulkiMapowanie.ToList()) //�eby nie modyfikowa� podczas iteracji
            {
                Kulka kulka = para.Key;
                PictureBox kulkaPB = para.Value;

                if (kulka.DoUsuni�cia)
                {
                    KulkaSpad�a(kulka);
                }
                else
                {
                    kulkaPB.Left = (int)kulka.Pozycja.X;
                    kulkaPB.Top = (int)kulka.Pozycja.Y;
                }
            }
        }

        private void AktualizujWizualizacjePrzeszkody()
        {
            foreach (var para in przeszkodyMapowanie.ToList())
            {
                Przeszkoda przeszkoda = para.Key;
                PictureBox przeszkodaPB = para.Value;

                if (przeszkoda.DoUsuni�cia)
                {
                    Controls.Remove(przeszkodaPB);
                    przeszkodyMapowanie.Remove(przeszkoda);;
                    wynik += przeszkoda.Nagroda ? przeszkoda.Bonus : 1;
                }
                else
                {
                    przeszkodaPB.Left = (int)przeszkoda.Obszar.Left;
                    przeszkodaPB.Top = (int)przeszkoda.Obszar.Top;
                    AktualizujLicznikNaEtykiecie(przeszkoda);
                }
            }
        }

        private void Koniec()
        {
            float dolnaGranicaY = ClientSize.Height;
            if (gra.CzyKoniecGry(dolnaGranicaY))
            {
                timer1.Stop();
                MessageBox.Show("Koniec gry! Wynik: " + wynik);
            }
        }

        //G��WNA P�TLA GRY
        private void timer1_Tick(object sender, EventArgs e)
        {
            gra.SymulujKrok();
            AktualizujWizualizacjeKulki();
            AktualizujWizualizacjePrzeszkody();
            Koniec();
        }
    }
}
