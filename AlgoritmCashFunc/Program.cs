using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Threading;
using System.Diagnostics;

namespace AlgoritmCashFunc
{
    static class Program
    {
        /// <summary>
        /// Флаг для работы сборщика муссора
        /// </summary>
        private static bool RunGC = true;

        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Проверка на то чтобы не запускался ещё один экземпляр нашего приложения
            if (Process.GetProcesses().Count(x => x.ProcessName == Process.GetCurrentProcess().ProcessName) > 1)
            {
                MessageBox.Show(String.Format("Приложение с именем {0}, уже работает на этом компьютере.", Process.GetCurrentProcess().ProcessName));
                Process.GetCurrentProcess().Kill();
            }

            bool Ishelp = false;
            string AutoStart = null;
            bool IsInterfase = true;

            //for (int i = 0; i < args.Length; i++)
            //{
            //    if (args[i] == @"-h" || args[i] == @"-H" || args[i] == @"-?" || args[i] == @"\h" || args[i] == @"\H" || args[i] == @"\?" || args[i] == @"/h" || args[i] == @"/H" || args[i] == @"/?") Ishelp = true;
            //    if (args[i] == @"-s" || args[i] == @"\s" || args[i] == @"/s" || args[i] == @"-S" || args[i] == @"\S" || args[i] == @"/S") { i++; AutoStart = args[i]; }
            //    if (args[i] == @"-i" || args[i] == @"\i" || args[i] == @"/i" || args[i] == @"-I" || args[i] == @"\I" || args[i] == @"/I") IsInterfase = false;
            //}

            try
            {
                // Проверка, если пользователь вызвал справку, то запускать прогу не надо
                if (Ishelp)
                {
                    Console.WriteLine(@"-s ShortName (Name Task Auto Run)");
                    Console.WriteLine(@"-i (not run interface)");
                }
                else
                {
                    // Проверка по процессам, чтобы приложение было в единственном экземпляре.
                    bool oneOnlyProg;
                    Mutex m = new Mutex(true, Application.ProductName, out oneOnlyProg);
                    if (oneOnlyProg == true || AutoStart != null)       // Если это автоматический запуск то можно запускать несколько экземпляров нашего приложения
                    {

                        // Инициализвция классов
                        Com.Log Log = new Com.Log("AlgoritmCashFunc.txt");
                        Com.Config Conf = new Com.Config("AlgoritmCashFunc.xml");
                        Com.UserFarm UFrm = new Com.UserFarm(15);

                        Application.EnableVisualStyles();
                        Application.SetCompatibleTextRenderingDefault(false);

                        // Проверка валидности лицензии
                        DialogResult rez = DialogResult.Yes;
                        while (!Com.Lic.isValid && rez == DialogResult.Yes)
                        {
                            if (!IsInterfase)
                            {
                                rez = DialogResult.No;
                                Console.WriteLine("У вас есть проблемы с лицензией. Обратитесь к вендору.");
                                if (!Ishelp) Com.Log.EventSave("У вас есть проблемы с лицензией. Обратитесь к вендору.", "Main", Lib.EventEn.Message);
                            }
                            else
                            {
                                Application.Run(new FLic());
                                //
                                if (!Com.Lic.isValid) rez = MessageBox.Show("Для запуска программы необходимо ввести лицензию, вы хотите это сделать?", "Лицензирование", MessageBoxButtons.YesNo);
                            }
                        }
                        // Проверка валидности лиценции
                        if (Com.Lic.isValid)
                        {
                            // Проверка режима с интерфейсом или нет
                            if (IsInterfase)
                            {
                                // Авторизуем пользователя
                                Login();

                                // Если мы авторизованны то запускаем приложение
                                if (Com.UserFarm.CurrentUser != null)
                                {
                                    // Асинхронный запуск процесса
                                    Thread thr = new Thread(GarbColRun);
                                    //thr = new Thread(new ParameterizedThreadStart(Run)); //Запуск с параметрами   
                                    thr.Name = "AE_Thr_GC";
                                    thr.IsBackground = true;
                                    thr.Start();

 
                                    //Делаем бесконечный цикл для основной менюшки для тогог чтобы реализовать блокировкуэкрана
                                    DialogResult rezExit = DialogResult.No;
                                    while (rezExit != DialogResult.OK)
                                    {
                                        // Пользователь авторизируется
                                        using (FStart Frm = new FStart())
                                        {
                                            // Запускаем форму и проверяем результат того что хочет сделать пользователь
                                            rezExit = Frm.ShowDialog();

                                            // Еcли пользователь не закрывает а просто выходит то у него взводится флаг в состояние OK и тогда спрашивать его логин нет необходимости просто закрываем прогу
                                            if (rezExit != DialogResult.OK) Login();
                                           
                                            // Если пользователь отказался от ввода пароля в форме логина и там выбрал что нужно закрыть прогу то закрываем наш цикл
                                            if (Com.UserFarm.CurrentUser == null) break;
                                        }
                                    }

                                    // Даём команду остановки
                                    Com.UserFarm.Stop();

                                    // Ожидаем завершения работы сборщика мусора
                                    RunGC = false;
                                    thr.Join();
                                    
                                    // Ожидаем завершения потока
                                    Com.UserFarm.Join();
                                }
                            }
                            else
                            {
                                // Авторизуемся под системной учёткой
                                Lib.User consUsr = new Lib.User("Console", "123456", "Console", Lib.RoleEn.Admin);
                                Com.UserFarm.List.Add(consUsr, true, false);
                                Com.UserFarm.SetupCurrentUser(consUsr, "123456");

                                // Если мы авторизованны то запускаем приложение
                                if (Com.UserFarm.CurrentUser != null)
                                {
                                    // Асинхронный запуск процесса
                                    Thread thr = new Thread(GarbColRun);
                                    //thr = new Thread(new ParameterizedThreadStart(Run)); //Запуск с параметрами   
                                    thr.Name = "AE_Thr_GC";
                                    thr.IsBackground = true;
                                    thr.Start();
                                }
                            }
                        }
                    }
                }
                // Тестируем список пользователей
                //Com.UserFarm.List.Add(new Lib.User("H","h","H", Lib.RoleEn.Admin));
                //foreach (Lib.User item in Com.UserFarm.List){}

                // Тестируем шифрование
                //string str = Com.Lic.InCode("1234ABC000845wferhyuy534tewgryyu7684regtrh7886756634t4gfy67865v35435465g5buvc45v4564563");
                //string stro = Com.Lic.DeCode(str);
                //bool f = Com.Lic.isValid;
                //string ActivN = Com.Lic.GetActivNumber();
                //string lic = Com.Lic.GetLicNumber(ActivN, "20180101", "Инфа", false);
                //Com.Lic.IsValidLicKey(lic);
                //Com.Lic.RegNewKey(lic,ActivN);
                //f = Com.Lic.isValid;

                // Тест провайдера
                //Lib.UProvider prv = new Lib.UProvider("ODBCprv", "cnstr");
                //List<string> str= Lib.UProvider.ListProviderName();


                // Тест получения списка пользователей
                //Com.CustomerFarm.List.ProcessingCustomers();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                if (!Ishelp) Com.Log.EventSave("Упали с ошибкой: " + ex.Message, "Main", Lib.EventEn.FatalError);
            }
            finally
            {
                if (!Ishelp) Com.Log.EventSave("Завершили работу программы.", "Main", Lib.EventEn.Message);
            }

        }

        private static void Login()
        {
            try
            {
                // Авторизуем пользователя
                DialogResult rezExit = DialogResult.None;
                while (Com.UserFarm.CurrentUser == null && rezExit == DialogResult.None)
                {
                    // Пользователь авторизируется
                    using (FLogon Frm = new FLogon())
                    {
                        rezExit = Frm.ShowDialog();

                        // Если пользователь с админской записью существует, то даём возможность авторизоваться
                        if (rezExit != DialogResult.OK && !Com.UserFarm.HashRoleUsers(Lib.RoleEn.Admin))
                        {
                            rezExit = MessageBox.Show("В системе нет ни одного администратора. Закрыть программу или войти под админской учёткой?", "Авторизация", MessageBoxButtons.OKCancel);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                Com.Log.EventSave("Упали с ошибкой: " + ex.Message, "Main", Lib.EventEn.FatalError);
            }
        }


        /// <summary>
        /// Асинхронный процесс сборщика мусора
        /// </summary>
        private static void GarbColRun()
        {
            int DefaultCountSec = 60 * 60;
            int CurCountSec = DefaultCountSec;
            while (RunGC)
            {
                if (CurCountSec > 0) { Thread.Sleep(1000); CurCountSec--; }
                else
                {
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    CurCountSec = DefaultCountSec;
                }
            }
        }
    }
}
