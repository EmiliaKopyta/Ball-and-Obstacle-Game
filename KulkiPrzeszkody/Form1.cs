using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace kulki
{
    public partial class Form1 : Form
    {
        private Dictionary<Kulka, PictureBox> kulkiMapowanie; //do œledzenia wizualizacji kulek
        private Dictionary<Przeszkoda, PictureBox> przeszkodyMapowanie; //do œledzenia wizualizacji przeszkód

        private int wynik = 0;
        private Gra gra;
        
        //jest tu du¿o parametrów, ¿eby ³atwo sterowaæ trudnoœci¹ i wygl¹dem gry
        //ustawione w odpowiednich funkcjach: 
        //pozycja startowa kulek, startX i startY w inicjalizacji przeszkód, iloœæ nowych rzêdów tworzonych podczas gry, kolory przeszkód/kulki

        private int szerokoœæPrzeszkody = 80;
        private int wysokoœæPrzeszkody = 35;
        private int maxLicznikPrzeszkody = 20;

        private int liczbaKolumnPrzeszkód = 7;
        private int liczbaWierszyPrzeszkód = 6;
        private int odstêpPrzeszkody = 10;

        private int odstêpNowePrzeszkody = 50;

        private int maxKulki = 30;
        private int rozmiarKulki = 10;
        private float predkoscKulki = 15f;

        private PointF punktStartowyKulka; //dla wszystkich kulek wspólny
        private int opóŸnienieRzutu = 200;
        private bool czyRzucanieTrwa = false;

        public Form1()
        {
            InitializeComponent();
            gra = new Gra(wysokoœæPrzeszkody+odstêpNowePrzeszkody); //min. o wysokoœæ, ¿eby przeszkody nie nachodzi³y na siebie i do tego dodajê odstêp
            kulkiMapowanie = new Dictionary<Kulka, PictureBox>();
            przeszkodyMapowanie = new Dictionary<Przeszkoda, PictureBox>();
            InicjalizujGranice(10, Color.DarkBlue);
            InicjalizujPrzeszkody(szerokoœæPrzeszkody, wysokoœæPrzeszkody, liczbaKolumnPrzeszkód, liczbaWierszyPrzeszkód);

            punktStartowyKulka = new PointF(ClientSize.Width / 2, ClientSize.Height - 50);
            InicjalizujKulki(rozmiarKulki);
        }

        //KULKI
        private void InicjalizujKulki(int rozmiarKulki)
        {
            for (int i = 0; i < maxKulki; i++)
            {
                Kulka nowaKulka = new Kulka(punktStartowyKulka, PointF.Empty, new SizeF(rozmiarKulki, rozmiarKulki));
                gra.DodajKulkê(nowaKulka);
                PictureBox nowaKulkaPB = new PictureBox
                {
                    Width = rozmiarKulki,
                    Height = rozmiarKulki,
                    BackColor = Color.Transparent, //przezroczysty, ¿eby tylko elipsa by³a widoczna
                    Location = new Point((int)punktStartowyKulka.X, (int)punktStartowyKulka.Y)
                };

                nowaKulkaPB.Paint += (sender, e) => //okr¹g³a kulka
                {
                    e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias; //wyg³adzanie krawêdzi
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
                await gra.RzuæKulki(kliknietaPozycja, punktStartowyKulka, predkoscKulki, opóŸnienieRzutu);

                czyRzucanieTrwa = false;
            }
        }

        //PRZESZKODY
        private Przeszkoda UtwórzPrzeszkodê(float x, float y, int szerokoœæ, int wysokoœæ)
        {
            Random random = new Random();
            var przeszkodaProstok¹tna = new RectangleF(x, y, szerokoœæ, wysokoœæ);
            var pictureBoxPrzeszkoda = new PictureBox
            {
                Width = szerokoœæ,
                Height = wysokoœæ,
                BackColor = Color.Blue,
                Location = new Point((int)przeszkodaProstok¹tna.Left, (int)przeszkodaProstok¹tna.Top),
                Tag = "przeszkoda"
            };

            int licznik = random.Next(1, maxLicznikPrzeszkody);
            var przeszkoda = new Przeszkoda(przeszkodaProstok¹tna, licznik);

            if (licznik % 5 == 0) //przyk³adowa nagroda
            {
                przeszkoda.UstawNagrodê(10);
                pictureBoxPrzeszkoda.BackColor = Color.Yellow;
            }

            gra.DodajPrzeszkodê(przeszkoda);
            Controls.Add(pictureBoxPrzeszkoda);
            przeszkodyMapowanie[przeszkoda] = pictureBoxPrzeszkoda;

            var labelLicznik = new Label
            {
                Text = licznik.ToString(),
                AutoSize = false,
                TextAlign = ContentAlignment.MiddleCenter,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                Width = szerokoœæ,
                Height = wysokoœæ,
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

        private void UtwórzRz¹dPrzeszkód(int startX, int startY, int szerokoœæ, int wysokoœæ, int liczbaKolumn)
        {
            for (int x = 0; x < liczbaKolumn; x++)
            {
                float przeszkodaX = startX + x * (szerokoœæ + odstêpPrzeszkody);
                UtwórzPrzeszkodê(przeszkodaX, startY, szerokoœæ, wysokoœæ);
            }
        }

        private void InicjalizujPrzeszkody(int szerokoœæ, int wysokoœæ, int liczbaKolumn, int liczbaWierszy)
        {
            int startX = (ClientSize.Width - (liczbaKolumn * (szerokoœæ + odstêpPrzeszkody))) / 2;
            int startY = 100;

            for (int y = 0; y < liczbaWierszy; y++)
            {
                int przeszkodaY = startY + y * (wysokoœæ + 10);
                UtwórzRz¹dPrzeszkód(startX, przeszkodaY, szerokoœæ, wysokoœæ, liczbaKolumn);
            }
        }

        //GRANICE
        private void UtwórzPrzeszkodêGranica(float x, float y, float szerokoœæ, float wysokoœæ, Color kolor, bool czyUsuwaKulkê = false)
        {
            var przeszkoda = new PrzeszkodaGranica(new RectangleF(x, y, szerokoœæ, wysokoœæ), czyUsuwaKulkê);
            var pictureBoxPrzeszkoda = new PictureBox
            {
                BackColor = kolor,
                Width = (int)szerokoœæ,
                Height = (int)wysokoœæ,
                Location = new Point((int)x, (int)y) //to potencjalnie daje b³êdy zaokr¹glenia, ale je akceptujê
            };

            //podobnie wygl¹da dodawanie znikaj¹cych przeszkód, ale przez ró¿ny typ 'przeszkoda' ciê¿ko wyodrêbniæ uniwersaln¹ funkcjê do powi¹zywania grafiki z logik¹ przeszkody bez wiêkszej komplikacji kodu
            Controls.Add(pictureBoxPrzeszkoda);
            gra.DodajPrzeszkodê(przeszkoda);
            przeszkodyMapowanie[przeszkoda] = pictureBoxPrzeszkoda;
        }

        private void InicjalizujGranice(float gruboœæGranicy, Color kolor)
        {
            UtwórzPrzeszkodêGranica(0, 0, gruboœæGranicy, ClientSize.Height, kolor); //lewa
            UtwórzPrzeszkodêGranica(ClientSize.Width - gruboœæGranicy, 0, gruboœæGranicy, ClientSize.Height, kolor); //prawa
            UtwórzPrzeszkodêGranica(0, 0, ClientSize.Width, gruboœæGranicy, kolor); //górna
            UtwórzPrzeszkodêGranica(0, ClientSize.Height - gruboœæGranicy, ClientSize.Width, gruboœæGranicy, kolor, true); //dolna granica z usuwaniem
        }


        //GRA
        private void KulkaSpad³a(Kulka kulka)
        {
            if (kulkiMapowanie.ContainsKey(kulka))
            {
                Controls.Remove(kulkiMapowanie[kulka]);
                kulkiMapowanie.Remove(kulka);
            }

            if (gra.WszystkieKulkiSpad³y)
                WszystkieKulkiSpad³y();
        }

        private void WszystkieKulkiSpad³y()
        {
            gra.ResetKulkiSpad³y();
            czyRzucanieTrwa = false;
            InicjalizujKulki(rozmiarKulki);
            PrzesuñPrzeszkodyWDó³();
            InicjalizujPrzeszkody(szerokoœæPrzeszkody, wysokoœæPrzeszkody, liczbaKolumnPrzeszkód, 1); //mo¿na dodaæ wiêcej kolumn i utrudniæ
        }

        private void PrzesuñPrzeszkodyWDó³()
        {
            foreach (var przeszkoda in gra.Przeszkody)
            {
                if (przeszkodyMapowanie.ContainsKey(przeszkoda))
                    przeszkodyMapowanie[przeszkoda].Location = new Point((int)przeszkoda.Obszar.Left, (int)przeszkoda.Obszar.Top);
            }
        }

        public void AktualizujWizualizacjeKulki()
        {
            foreach (var para in kulkiMapowanie.ToList()) //¿eby nie modyfikowaæ podczas iteracji
            {
                Kulka kulka = para.Key;
                PictureBox kulkaPB = para.Value;

                if (kulka.DoUsuniêcia)
                {
                    KulkaSpad³a(kulka);
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

                if (przeszkoda.DoUsuniêcia)
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

        //G£ÓWNA PÊTLA GRY
        private void timer1_Tick(object sender, EventArgs e)
        {
            gra.SymulujKrok();
            AktualizujWizualizacjeKulki();
            AktualizujWizualizacjePrzeszkody();
            Koniec();
        }
    }
}
