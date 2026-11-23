using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

class Program
{
    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int touche);

    static bool Appuye(int touche)
    {
        return (GetAsyncKeyState(touche) & 0x8000) != 0;
    }

    [DllImport("user32.dll")]
    static extern void keybd_event(byte touche, byte scan, uint flags, UIntPtr info);

    static void PleinEcran()
    {
        keybd_event(0x12, 0, 0, UIntPtr.Zero);
        keybd_event(0x0D, 0, 0, UIntPtr.Zero);
        keybd_event(0x0D, 0, 2, UIntPtr.Zero);
        keybd_event(0x12, 0, 2, UIntPtr.Zero);
        Thread.Sleep(500);
    }

    static void Main()
    {
        Console.OutputEncoding = Encoding.UTF8;
        Console.CursorVisible = false;

        PleinEcran();
        Console.Clear();

        int largeur = Console.WindowWidth - 1;
        int hauteur = Console.WindowHeight - 1;
        int tailleRaquette = Math.Max(4, hauteur / 6);
        double vitesseRaquette = hauteur / 50.0;

        double j1 = hauteur / 2 - tailleRaquette / 2;
        double j2 = j1;
        int score1 = 0;
        int score2 = 0;
        int scoreMax = 3;

        double balleX = largeur / 2;
        double balleY = hauteur / 2;
        double vitesseX = 0.5;
        double vitesseY = 0.25;

        while (!Appuye(0x1B) && score1 < scoreMax && score2 < scoreMax)
        {
            if (Appuye('Z')) j1 -= vitesseRaquette;
            if (Appuye('S')) j1 += vitesseRaquette;
            if (Appuye(0x26)) j2 -= vitesseRaquette;
            if (Appuye(0x28)) j2 += vitesseRaquette;

            j1 = Math.Clamp(j1, 0, hauteur - tailleRaquette);
            j2 = Math.Clamp(j2, 0, hauteur - tailleRaquette);

            int r1 = (int)Math.Round(j1);
            int r2 = (int)Math.Round(j2);

            balleX += vitesseX;
            balleY += vitesseY;

            if (balleY < 0)
            {
                balleY = 0;
                vitesseY = -vitesseY;
            }

            if (balleY > hauteur - 1)
            {
                balleY = hauteur - 1;
                vitesseY = -vitesseY;
            }

            int by = (int)Math.Round(balleY);

            if (vitesseX < 0 && balleX <= 3 && balleX > 1.5 && by >= r1 && by < r1 + tailleRaquette)
            {
                balleX = 3;
                vitesseX = Math.Min(-vitesseX * 1.1, 1.0);
            }

            if (vitesseX > 0 && balleX >= largeur - 4 && balleX < largeur - 2.5 && by >= r2 && by < r2 + tailleRaquette)
            {
                balleX = largeur - 4;
                vitesseX = -Math.Min(vitesseX * 1.1, 1.0);
            }

            if (balleX < 0)
            {
                score2++;
                balleX = largeur / 2;
                balleY = hauteur / 2;
                vitesseX = 0.5;
            }

            if (balleX > largeur - 1)
            {
                score1++;
                balleX = largeur / 2;
                balleY = hauteur / 2;
                vitesseX = -0.5;
            }

            int bx = (int)Math.Round(balleX);
            by = (int)Math.Round(balleY);

            StringBuilder ecran = new StringBuilder();

            for (int y = 0; y < hauteur; y++)
            {
                for (int x = 0; x < largeur; x++)
                {
                    if (x == bx && y == by)
                        ecran.Append('█');
                    else if (x == 2 && y >= r1 && y < r1 + tailleRaquette)
                        ecran.Append('█');
                    else if (x == largeur - 3 && y >= r2 && y < r2 + tailleRaquette)
                        ecran.Append('█');
                    else if (x == largeur / 2 && y % 2 == 0)
                        ecran.Append('█');
                    else
                        ecran.Append(' ');
                }
                ecran.Append('\n');
            }

            ecran.Append("        JOUEUR 1 : " + score1 + "          JOUEUR 2 : " + score2 + "     ");

            Console.SetCursorPosition(0, 0);
            Console.Write(ecran.ToString());

            Thread.Sleep(16);
        }

        if (score1 >= scoreMax || score2 >= scoreMax)
        {
            string gagnant = score1 > score2 ? "JOUEUR 1 GAGNE !" : "JOUEUR 2 GAGNE !";
            Console.SetCursorPosition(largeur / 2 - gagnant.Length / 2, hauteur / 2);
            Console.Write(gagnant);
            Thread.Sleep(1000);
            while (Console.KeyAvailable) Console.ReadKey(true);
            Console.ReadKey(true);
        }
    }
}
