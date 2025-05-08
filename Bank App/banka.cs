using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;

namespace ConsoleApp1
{
    public class banka
    {
        private MySqlConnection mysql;
        private int aktif_musteri = 0;
        private double kasa = 200;

        /* program.cs ye yazılacak kod
            Console.WriteLine("Lütfen şifrenizi giriniz : ");
            string sifre = Console.ReadLine();
            banka b = new banka(sifre);
        */

        public banka(string sifre)
        {

            mysql = new MySqlConnection("Server=localhost;DataBase=banka;Uid=root;Pwd=''");
            mysql.Open();

            if (mysql.State == System.Data.ConnectionState.Open)
            {
                Console.WriteLine("Veri tabanına bağlantı yapıldı");
                this.Sifre_Dogrula(sifre);
            }
            else
            {
                Console.WriteLine("Veri tabanına bağlantı yapılamadı");
            }
        }

        public void Sifre_Dogrula(string sifre)
        {
            string query = "SELECT id FROM musteriler WHERE sifre = @sifre";
            MySqlCommand cmd = new MySqlCommand(query, mysql);
            cmd.Parameters.AddWithValue("@sifre", sifre);

            object result = cmd.ExecuteScalar();

            if (result != null)
            {
                aktif_musteri = Convert.ToInt32(result);
                this.menu_Olustur();
            }
            else
            {
                Console.WriteLine("Şifre hatalı");
            }
            mysql.Close();
        }

        public void menu_Olustur()
        {
            Console.Clear();
            Console.WriteLine("1- Bakiye Sorgula");
            Console.WriteLine("2- Para Çek");
            Console.WriteLine("3- Para Yatır");
            Console.WriteLine("4- Havale Yap");
            Console.WriteLine("5- Çıkış");
            string m = Console.ReadLine();
            int havaleHesap = 0;
            double miktar = 0;

            switch (m)
            {
                case "1":
                    this.BakiyeSorgula();
                    break;
                case "2":
                    Console.Clear();
                    Console.WriteLine("Lütfen çekeceğiniz miktarı yazınız : ");
                    miktar = Convert.ToDouble(Console.ReadLine());
                    this.ParaCek(miktar);
                    break;
                case "3":
                    Console.Clear();
                    Console.WriteLine("Lütfen yatıracağınız miktarı yazınız : ");
                    miktar = Convert.ToDouble(Console.ReadLine());
                    this.ParaYatir(miktar);
                    break;
                case "4":
                    Console.Clear();
                    Console.WriteLine("Lütfen Havale yapacağınız hesabı yazınız : ");
                    havaleHesap = Convert.ToInt32(Console.ReadLine());
                    Console.WriteLine("Lütfen Havale yapacağınız miktarı yazınız : ");
                    miktar = Convert.ToDouble(Console.ReadLine());
                    this.HavaleYap(havaleHesap, miktar);
                    break;

                case "5":
                    this.cikis();
                    break;
                default:
                    Console.WriteLine("Geçersiz bir seçim yaptınız");
                    break;
            }
        }

        public void BakiyeSorgula()
        {
            string query = "SELECT bakiye FROM musteriler WHERE id = @id";
            MySqlCommand cmd = new MySqlCommand(query, mysql);
            cmd.Parameters.AddWithValue("@id", aktif_musteri);

            object result = cmd.ExecuteScalar();
            double bakiye = Convert.ToDouble(result);

            Console.Clear();
            Console.WriteLine("Bakiyeniz : " + bakiye);
            Console.WriteLine("Ana menüye dönmek için 1'e basınız");
            string t = Console.ReadLine();
            if (t == "1")
            {
                this.menu_Olustur();
            }
        }

        public void ParaCek(double miktar)
        {
            string query = "SELECT bakiye FROM musteriler WHERE id = @id";
            MySqlCommand cmd = new MySqlCommand(query, mysql);
            cmd.Parameters.AddWithValue("@id", aktif_musteri);

            object result = cmd.ExecuteScalar();
            double bakiye = Convert.ToDouble(result);

            Console.Clear();
            if (miktar > kasa)
            {
                Console.WriteLine("Kasada o kadar para yok!");
            }
            else if (miktar > bakiye)
            {
                Console.WriteLine("Hesabınızda o kadar para yok!");
            }
            else
            {
                bakiye -= miktar;
                kasa -= miktar;

                string updateQuery = "UPDATE musteriler SET bakiye = @bakiye WHERE id = @id";
                MySqlCommand updateCmd = new MySqlCommand(updateQuery, mysql);
                updateCmd.Parameters.AddWithValue("@bakiye", bakiye);
                updateCmd.Parameters.AddWithValue("@id", aktif_musteri);
                updateCmd.ExecuteNonQuery();
            }

            Console.WriteLine("Ana menüye dönmek için 1'e basınız");
            string t = Console.ReadLine();
            if (t == "1")
            {
                this.menu_Olustur();
            }
        }

        public void ParaYatir(double miktar)
        {
            string query = "SELECT bakiye FROM musteriler WHERE id = @id";
            MySqlCommand cmd = new MySqlCommand(query, mysql);
            cmd.Parameters.AddWithValue("@id", aktif_musteri);

            object result = cmd.ExecuteScalar();
            double bakiye = Convert.ToDouble(result);

            Console.Clear();
            bakiye += miktar;
            kasa += miktar;

            string updateQuery = "UPDATE musteriler SET bakiye = @bakiye WHERE id = @id";
            MySqlCommand updateCmd = new MySqlCommand(updateQuery, mysql);
            updateCmd.Parameters.AddWithValue("@bakiye", bakiye);
            updateCmd.Parameters.AddWithValue("@id", aktif_musteri);
            updateCmd.ExecuteNonQuery();

            Console.WriteLine("Ana menüye dönmek için 1'e basınız");
            string t = Console.ReadLine();
            if (t == "1")
            {
                this.menu_Olustur();
            }
        }

        public void HavaleYap(int hesap_no, double miktar)
        {
            
            string query = "SELECT bakiye FROM musteriler WHERE id = @id";
            MySqlCommand cmd = new MySqlCommand(query, mysql);
            cmd.Parameters.AddWithValue("@id", aktif_musteri);

            object result = cmd.ExecuteScalar();
            double bakiye = Convert.ToDouble(result);

            Console.Clear();

            
            string targetQuery = "SELECT id, bakiye FROM musteriler WHERE id = @id";
            MySqlCommand targetCmd = new MySqlCommand(targetQuery, mysql);
            targetCmd.Parameters.AddWithValue("@id", hesap_no);

            
            double targetBakiye = 0;
            using (MySqlDataReader reader = targetCmd.ExecuteReader())
            {
                if (reader.Read())
                {
                    targetBakiye = reader.GetDouble("bakiye");
                }
                else
                {
                    Console.WriteLine("Hesap bulunamadı");
                    return;
                }
            }

            
            if (miktar > kasa)
            {
                Console.WriteLine("Kasada o kadar para yok!");
            }
            else if (miktar > bakiye)
            {
                Console.WriteLine("Hesabınızda o kadar para yok!");
            }
            else
            {
               
                targetBakiye += miktar;
                bakiye -= miktar;

                
                string updateQueryTarget = "UPDATE musteriler SET bakiye = @bakiye WHERE id = @id";
                MySqlCommand updateCmdTarget = new MySqlCommand(updateQueryTarget, mysql);
                updateCmdTarget.Parameters.AddWithValue("@bakiye", targetBakiye);
                updateCmdTarget.Parameters.AddWithValue("@id", hesap_no);
                updateCmdTarget.ExecuteNonQuery();

                
                string updateQuerySender = "UPDATE musteriler SET bakiye = @bakiye WHERE id = @id";
                MySqlCommand updateCmdSender = new MySqlCommand(updateQuerySender, mysql);
                updateCmdSender.Parameters.AddWithValue("@bakiye", bakiye);
                updateCmdSender.Parameters.AddWithValue("@id", aktif_musteri);
                updateCmdSender.ExecuteNonQuery();

                Console.WriteLine("Para transferi gerçekleşti");
            }

            
            Console.WriteLine("Ana menüye dönmek için 1'e basınız");
            string t = Console.ReadLine();
            if (t == "1")
            {
                this.menu_Olustur();
            }
        }


        public void cikis()
        {
            Environment.Exit(0);
        }
    }
}

