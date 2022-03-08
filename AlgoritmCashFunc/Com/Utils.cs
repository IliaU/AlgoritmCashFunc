using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AlgoritmCashFunc.Lib;

namespace AlgoritmCashFunc.Com
{
    /// <summary>
    /// Утилиты
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Для возврата числа в виде строки а не цифры
        /// </summary>
        /// <param name="d">Число которое надо перевести в текст</param>
        /// <param name="str1">Окончание которое подставлять например 1 рубль</param>
        /// <param name="str2">Окончание которое подставлять например 2 рубля</param>
        /// <param name="str5">Окончание которое подставлять например 1 рублей</param>
        /// <param name="IsShen">Способ расчёта например для рублей двА рубря а для копеек двЕ копейки если чтоит true значет действуем по женскому роду как для копеек</param>
        /// <returns>Возвращает строковое значение числа</returns>
        public static string GetStringForInt(int d, string str1, string str2, string str5, bool IsShen)
        {
            try
            {
                string rez=string.Empty;

                char[] dChr = d.ToString().ToCharArray();

                // если у нас только один символ и он равен 0 то пишем ноль и дальше делать ничего не надо
                if (dChr.Length==1 && dChr[0].ToString() == "0")
                {
                    rez = "ноль";
                }

                List<string> parse3simvol = new List<string>();

                // делим по 3 символа
                switch (dChr.Length)
                {
                    case 1:
                        parse3simvol.Add(dChr[0].ToString());
                        break;
                    case 2:
                        parse3simvol.Add(string.Format("{0}{1}",dChr[0].ToString(), dChr[1].ToString()));
                        break;
                    case 3:
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[0].ToString(), dChr[1].ToString(), dChr[2].ToString()));
                        break;
                    case 4:
                        parse3simvol.Add(dChr[0].ToString());
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[1].ToString(), dChr[2].ToString(), dChr[3].ToString()));
                        break;
                    case 5:
                        parse3simvol.Add(string.Format("{0}{1}", dChr[0].ToString(), dChr[1].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[2].ToString(), dChr[3].ToString(), dChr[4].ToString()));
                        break;
                    case 6:
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[0].ToString(), dChr[1].ToString(), dChr[2].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[3].ToString(), dChr[4].ToString(), dChr[5].ToString()));
                        break;
                    case 7:
                        parse3simvol.Add(dChr[0].ToString());
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[1].ToString(), dChr[2].ToString(), dChr[3].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[4].ToString(), dChr[5].ToString(), dChr[6].ToString()));
                        break;
                    case 8:
                        parse3simvol.Add(string.Format("{0}{1}", dChr[0].ToString(), dChr[1].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[2].ToString(), dChr[3].ToString(), dChr[4].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[5].ToString(), dChr[6].ToString(), dChr[7].ToString()));
                        break;
                    case 9:
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[0].ToString(), dChr[1].ToString(), dChr[2].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[3].ToString(), dChr[4].ToString(), dChr[5].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[6].ToString(), dChr[7].ToString(), dChr[8].ToString()));
                        break;
                    case 10:
                        parse3simvol.Add(dChr[0].ToString());
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[1].ToString(), dChr[2].ToString(), dChr[3].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[4].ToString(), dChr[5].ToString(), dChr[6].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[7].ToString(), dChr[8].ToString(), dChr[9].ToString()));
                        break;
                    case 11:
                        parse3simvol.Add(string.Format("{0}{1}", dChr[0].ToString(), dChr[1].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[2].ToString(), dChr[3].ToString(), dChr[4].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[5].ToString(), dChr[6].ToString(), dChr[7].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[8].ToString(), dChr[9].ToString(), dChr[10].ToString()));
                        break;
                    case 12:
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[0].ToString(), dChr[1].ToString(), dChr[2].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[3].ToString(), dChr[4].ToString(), dChr[5].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[6].ToString(), dChr[7].ToString(), dChr[8].ToString()));
                        parse3simvol.Add(string.Format("{0}{1}{2}", dChr[9].ToString(), dChr[10].ToString(), dChr[11].ToString()));
                        break;
                    default:
                        break;
                }

                // Начинаем парсить
                for (int i = 0; i < parse3simvol.Count; i++)
                {
                    string s1 = string.Empty;
                    string s2 = string.Empty;
                    if (parse3simvol[i].Length == 3)
                    {
                        s1 = parse3simvol[i].Substring(0, 1);
                        s2 = parse3simvol[i].Substring(1, 2);
                    }
                    else
                    {
                        s2 = parse3simvol[i];
                    }

                    switch (s1)
                    {
                        case "1":
                            rez += (rez == string.Empty ? "" : " ") + "сто";
                            break;
                        case "2":
                            rez += (rez == string.Empty ? "" : " ") + "двести";
                            break;
                        case "3":
                            rez += (rez == string.Empty ? "" : " ") + "тристо";
                            break;
                        case "4":
                            rez += (rez == string.Empty ? "" : " ") + "четыресто";
                            break;
                        case "5":
                            rez += (rez == string.Empty ? "" : " ") + "пятьсот";
                            break;
                        case "6":
                            rez += (rez == string.Empty ? "" : " ") + "шестьсот";
                            break;
                        case "7":
                            rez += (rez == string.Empty ? "" : " ") + "семьсот";
                            break;
                        case "8":
                            rez += (rez == string.Empty ? "" : " ") + "восемьсот";
                            break;
                        case "9":
                            rez += (rez == string.Empty ? "" : " ") + "девятьсот";
                            break;
                        default:
                            break;
                    }

                    switch (s2)
                    {
                        case "01":
                        case "1":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "одина";
                            else rez += (rez == string.Empty ? "" : " ") + "один";
                            break;
                        case "02":
                        case "2":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "две";
                            else rez += (rez == string.Empty ? "" : " ") + "два";
                            break;
                        case "03":
                        case "3":
                            rez += (rez == string.Empty ? "" : " ") + "три";
                            break;
                        case "04":
                        case "4":
                            rez += (rez == string.Empty ? "" : " ") + "четыре";
                            break;
                        case "05":
                        case "5":
                            rez += (rez == string.Empty ? "" : " ") + "пять";
                            break;
                        case "06":
                        case "6":
                            rez += (rez == string.Empty ? "" : " ") + "шесть";
                            break;
                        case "07":
                        case "7":
                            rez += (rez == string.Empty ? "" : " ") + "семь";
                            break;
                        case "08":
                        case "8":
                            rez += (rez == string.Empty ? "" : " ") + "восемь";
                            break;
                        case "09":
                        case "9":
                            rez += (rez == string.Empty ? "" : " ") + "девять";
                            break;
                        case "10":
                            rez += (rez == string.Empty ? "" : " ") + "десять";
                            break;
                        case "11":
                            rez += (rez == string.Empty ? "" : " ") + "одиннадцать";
                            break;
                        case "12":
                            rez += (rez == string.Empty ? "" : " ") + "двенадцать";
                            break;
                        case "13":
                            rez += (rez == string.Empty ? "" : " ") + "тренадцать";
                            break;
                        case "14":
                            rez += (rez == string.Empty ? "" : " ") + "четырнадцать";
                            break;
                        case "15":
                            rez += (rez == string.Empty ? "" : " ") + "пятнадцать";
                            break;
                        case "16":
                            rez += (rez == string.Empty ? "" : " ") + "шестнадцать";
                            break;
                        case "17":
                            rez += (rez == string.Empty ? "" : " ") + "семнадцать";
                            break;
                        case "18":
                            rez += (rez == string.Empty ? "" : " ") + "восемьнадцать";
                            break;
                        case "19":
                            rez += (rez == string.Empty ? "" : " ") + "девятнадцать";
                            break;
                        case "20":
                            rez += (rez == string.Empty ? "" : " ") + "двадцать";
                            break;
                        case "21":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "двадцать одина";
                            else rez += (rez == string.Empty ? "" : " ") + "двадцать один";
                            break;
                        case "22":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "двадцать две";
                            else rez += (rez == string.Empty ? "" : " ") + "двадцать два";
                            break;
                        case "23":
                            rez += (rez == string.Empty ? "" : " ") + "двадцать три";
                            break;
                        case "24":
                            rez += (rez == string.Empty ? "" : " ") + "двадцать четыре";
                            break;
                        case "25":
                            rez += (rez == string.Empty ? "" : " ") + "двадцать пять";
                            break;
                        case "26":
                            rez += (rez == string.Empty ? "" : " ") + "двадцать шесть";
                            break;
                        case "27":
                            rez += (rez == string.Empty ? "" : " ") + "двадцать семь";
                            break;
                        case "28":
                            rez += (rez == string.Empty ? "" : " ") + "двадцать восемь";
                            break;
                        case "29":
                            rez += (rez == string.Empty ? "" : " ") + "двадцать девять";
                            break;
                        case "30":
                            rez += (rez == string.Empty ? "" : " ") + "тридцать";
                            break;
                        case "31":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "тридцать одина";
                            else rez += (rez == string.Empty ? "" : " ") + "тридцать один";
                            break;
                        case "32":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "тридцать две";
                            else rez += (rez == string.Empty ? "" : " ") + "тридцать два";
                            break;
                        case "33":
                            rez += (rez == string.Empty ? "" : " ") + "тридцать три";
                            break;
                        case "34":
                            rez += (rez == string.Empty ? "" : " ") + "тридцать четыре";
                            break;
                        case "35":
                            rez += (rez == string.Empty ? "" : " ") + "тридцать пять";
                            break;
                        case "36":
                            rez += (rez == string.Empty ? "" : " ") + "тридцать шесть";
                            break;
                        case "37":
                            rez += (rez == string.Empty ? "" : " ") + "тридцать семь";
                            break;
                        case "38":
                            rez += (rez == string.Empty ? "" : " ") + "тридцать восемь";
                            break;
                        case "39":
                            rez += (rez == string.Empty ? "" : " ") + "тридцать девять";
                            break;
                        case "40":
                            rez += (rez == string.Empty ? "" : " ") + "сорок";
                            break;
                        case "41":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "сорок одина";
                            else rez += (rez == string.Empty ? "" : " ") + "сорок один";
                            break;
                        case "42":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "сорок две";
                            else rez += (rez == string.Empty ? "" : " ") + "сорок два";
                            break;
                        case "43":
                            rez += (rez == string.Empty ? "" : " ") + "сорок три";
                            break;
                        case "44":
                            rez += (rez == string.Empty ? "" : " ") + "сорок четыре";
                            break;
                        case "45":
                            rez += (rez == string.Empty ? "" : " ") + "сорок пять";
                            break;
                        case "46":
                            rez += (rez == string.Empty ? "" : " ") + "сорок шесть";
                            break;
                        case "47":
                            rez += (rez == string.Empty ? "" : " ") + "сорок семь";
                            break;
                        case "48":
                            rez += (rez == string.Empty ? "" : " ") + "сорок восемь";
                            break;
                        case "49":
                            rez += (rez == string.Empty ? "" : " ") + "сорок девять";
                            break;
                        case "50":
                            rez += (rez == string.Empty ? "" : " ") + "пятьдесят";
                            break;
                        case "51":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "пятьдесят одина";
                            else rez += (rez == string.Empty ? "" : " ") + "пятьдесят один";
                            break;
                        case "52":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "пятьдесят две";
                            else rez += (rez == string.Empty ? "" : " ") + "пятьдесят два";
                            break;
                        case "53":
                            rez += (rez == string.Empty ? "" : " ") + "пятьдесят три";
                            break;
                        case "54":
                            rez += (rez == string.Empty ? "" : " ") + "пятьдесят четыре";
                            break;
                        case "55":
                            rez += (rez == string.Empty ? "" : " ") + "пятьдесят пять";
                            break;
                        case "56":
                            rez += (rez == string.Empty ? "" : " ") + "пятьдесят шесть";
                            break;
                        case "57":
                            rez += (rez == string.Empty ? "" : " ") + "пятьдесят семь";
                            break;
                        case "58":
                            rez += (rez == string.Empty ? "" : " ") + "пятьдесят восемь";
                            break;
                        case "59":
                            rez += (rez == string.Empty ? "" : " ") + "пятьдесят девять";
                            break;
                        case "60":
                            rez += (rez == string.Empty ? "" : " ") + "шестьдесят";
                            break;
                        case "61":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "шестьдесят одна";
                            else rez += (rez == string.Empty ? "" : " ") + "шестьдесят один";
                            break;
                        case "62":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "шестьдесят две";
                            else rez += (rez == string.Empty ? "" : " ") + "шестьдесят два";
                            break;
                        case "63":
                            rez += (rez == string.Empty ? "" : " ") + "шестьдесят три";
                            break;
                        case "64":
                            rez += (rez == string.Empty ? "" : " ") + "шестьдесят четыре";
                            break;
                        case "65":
                            rez += (rez == string.Empty ? "" : " ") + "шестьдесят пять";
                            break;
                        case "66":
                            rez += (rez == string.Empty ? "" : " ") + "шестьдесят шесть";
                            break;
                        case "67":
                            rez += (rez == string.Empty ? "" : " ") + "шестьдесят семь";
                            break;
                        case "68":
                            rez += (rez == string.Empty ? "" : " ") + "шестьдесят восемь";
                            break;
                        case "69":
                            rez += (rez == string.Empty ? "" : " ") + "шестьдесят девять";
                            break;
                        case "70":
                            rez += (rez == string.Empty ? "" : " ") + "семдесят";
                            break;
                        case "71":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "семдесят одина";
                            else rez += (rez == string.Empty ? "" : " ") + "семдесят один";
                            break;
                        case "72":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "семдесят две";
                            else rez += (rez == string.Empty ? "" : " ") + "семдесят два";
                            break;
                        case "73":
                            rez += (rez == string.Empty ? "" : " ") + "семдесят три";
                            break;
                        case "74":
                            rez += (rez == string.Empty ? "" : " ") + "семдесят четыре";
                            break;
                        case "75":
                            rez += (rez == string.Empty ? "" : " ") + "семдесят пять";
                            break;
                        case "76":
                            rez += (rez == string.Empty ? "" : " ") + "семдесят шесть";
                            break;
                        case "77":
                            rez += (rez == string.Empty ? "" : " ") + "семдесят семь";
                            break;
                        case "78":
                            rez += (rez == string.Empty ? "" : " ") + "семдесят восемь";
                            break;
                        case "79":
                            rez += (rez == string.Empty ? "" : " ") + "семдесят девять";
                            break;
                        case "80":
                            rez += (rez == string.Empty ? "" : " ") + "восемьдесят";
                            break;
                        case "81":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "восемьдесят одина";
                            else rez += (rez == string.Empty ? "" : " ") + "восемьдесят один";
                            break;
                        case "82":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "восемьдесят две";
                            else rez += (rez == string.Empty ? "" : " ") + "восемьдесят два";
                            break;
                        case "83":
                            rez += (rez == string.Empty ? "" : " ") + "восемьдесят три";
                            break;
                        case "84":
                            rez += (rez == string.Empty ? "" : " ") + "восемьдесят четыре";
                            break;
                        case "85":
                            rez += (rez == string.Empty ? "" : " ") + "восемьдесят пять";
                            break;
                        case "86":
                            rez += (rez == string.Empty ? "" : " ") + "восемьдесят шесть";
                            break;
                        case "87":
                            rez += (rez == string.Empty ? "" : " ") + "восемьдесят семь";
                            break;
                        case "88":
                            rez += (rez == string.Empty ? "" : " ") + "восемьдесят восемь";
                            break;
                        case "89":
                            rez += (rez == string.Empty ? "" : " ") + "восемьдесят девять";
                            break;
                        case "90":
                            rez += (rez == string.Empty ? "" : " ") + "девяносто";
                            break;
                        case "91":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "девяносто одна";
                            else rez += (rez == string.Empty ? "" : " ") + "девяносто один";
                            break;
                        case "92":
                            if (IsShen || parse3simvol.Count == 2) rez += (rez == string.Empty ? "" : " ") + "девяносто две";
                            else rez += (rez == string.Empty ? "" : " ") + "девяносто два";
                            break;
                        case "93":
                            rez += (rez == string.Empty ? "" : " ") + "девяносто три";
                            break;
                        case "94":
                            rez += (rez == string.Empty ? "" : " ") + "девяносто четыре";
                            break;
                        case "95":
                            rez += (rez == string.Empty ? "" : " ") + "девяносто пять";
                            break;
                        case "96":
                            rez += (rez == string.Empty ? "" : " ") + "девяносто шесть";
                            break;
                        case "97":
                            rez += (rez == string.Empty ? "" : " ") + "девяносто семь";
                            break;
                        case "98":
                            rez += (rez == string.Empty ? "" : " ") + "девяносто восемь";
                            break;
                        case "99":
                            rez += (rez == string.Empty ? "" : " ") + "девяносто девять";
                            break;
                        default:
                            break;
                    }

                    switch (parse3simvol.Count - i)
                    {
                        case 1:
                            switch (s2)
                            {
                                case "01":
                                case "1":
                                case "21":
                                case "31":
                                case "41":
                                case "51":
                                case "61":
                                case "71":
                                case "81":
                                case "91":
                                    rez += string.Format(" {0}", str1);
                                    break;
                                case "02":
                                case "2":
                                case "22":
                                case "32":
                                case "42":
                                case "52":
                                case "62":
                                case "72":
                                case "82":
                                case "92":
                                case "03":
                                case "3":
                                case "23":
                                case "33":
                                case "43":
                                case "53":
                                case "63":
                                case "73":
                                case "83":
                                case "93":
                                case "04":
                                case "4":
                                case "24":
                                case "34":
                                case "44":
                                case "54":
                                case "64":
                                case "74":
                                case "84":
                                case "94":
                                    rez += string.Format(" {0}", str2);
                                    break;
                                default:
                                    rez += string.Format(" {0}", str5);
                                    break;
                            }
                            break;

                        case 2:
                            if (string.Format("{0}{1}", s1, s2) != "000")
                            {
                                switch (s2)
                                {
                                    case "01":
                                    case "1":
                                    case "21":
                                    case "31":
                                    case "41":
                                    case "51":
                                    case "61":
                                    case "71":
                                    case "81":
                                    case "91":
                                        rez += " тысяча";
                                        break;
                                    case "02":
                                    case "2":
                                    case "22":
                                    case "32":
                                    case "42":
                                    case "52":
                                    case "62":
                                    case "72":
                                    case "82":
                                    case "92":
                                    case "03":
                                    case "3":
                                    case "23":
                                    case "33":
                                    case "43":
                                    case "53":
                                    case "63":
                                    case "73":
                                    case "83":
                                    case "93":
                                    case "04":
                                    case "4":
                                    case "24":
                                    case "34":
                                    case "44":
                                    case "54":
                                    case "64":
                                    case "74":
                                    case "84":
                                    case "94":
                                        rez += " тысячи";
                                        break;
                                    default:
                                        rez += " тысяч";
                                        break;
                                }
                            }
                            break;
                        case 3:
                            if (string.Format("{0}{1}", s1, s2) != "000")
                            {
                                switch (s2)
                                {
                                    case "01":
                                    case "1":
                                    case "21":
                                    case "31":
                                    case "41":
                                    case "51":
                                    case "61":
                                    case "71":
                                    case "81":
                                    case "91":
                                        rez += " миллион";
                                        break;
                                    case "02":
                                    case "2":
                                    case "22":
                                    case "32":
                                    case "42":
                                    case "52":
                                    case "62":
                                    case "72":
                                    case "82":
                                    case "92":
                                    case "03":
                                    case "3":
                                    case "23":
                                    case "33":
                                    case "43":
                                    case "53":
                                    case "63":
                                    case "73":
                                    case "83":
                                    case "93":
                                    case "04":
                                    case "4":
                                    case "24":
                                    case "34":
                                    case "44":
                                    case "54":
                                    case "64":
                                    case "74":
                                    case "84":
                                    case "94":
                                        rez += " миллиона";
                                        break;
                                    default:
                                        rez += " миллионов";
                                        break;
                                }
                            }
                            break;
                        case 4:
                            if (string.Format("{0}{1}", s1, s2) != "000")
                            {
                                switch (s2)
                                {
                                    case "01":
                                    case "1":
                                    case "21":
                                    case "31":
                                    case "41":
                                    case "51":
                                    case "61":
                                    case "71":
                                    case "81":
                                    case "91":
                                        rez += " миллиард";
                                        break;
                                    case "02":
                                    case "2":
                                    case "22":
                                    case "32":
                                    case "42":
                                    case "52":
                                    case "62":
                                    case "72":
                                    case "82":
                                    case "92":
                                    case "03":
                                    case "3":
                                    case "23":
                                    case "33":
                                    case "43":
                                    case "53":
                                    case "63":
                                    case "73":
                                    case "83":
                                    case "93":
                                    case "04":
                                    case "4":
                                    case "24":
                                    case "34":
                                    case "44":
                                    case "54":
                                    case "64":
                                    case "74":
                                    case "84":
                                    case "94":
                                        rez += " миллиарда";
                                        break;
                                    default:
                                        rez += " миллиардов";
                                        break;
                                }
                            }
                            break;
                        default:
                            break;
                    }

                }

                // Первый символ делаем заглавным
                string rez1 = rez.Substring(0, 1);
                string rez2 = rez.Substring(1);
                rez = string.Format("{0}{1}", rez1.ToUpper(), rez2);

                return rez;
            }
            catch (Exception ex)
            {
                ApplicationException ae = new ApplicationException(string.Format("Упали при вызове метода с ошибкой: ({0})", ex.Message));
                Log.EventSave(ae.Message, "Utils.GetStringForDouble", EventEn.Error);
                throw ae;
            }
        }
    }
}
