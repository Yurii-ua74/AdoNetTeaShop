using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace AdoNetTeaShop
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private SqlConnection connect = new SqlConnection();
        SqlDataAdapter adapt = null;
        DataSet set = null;
        SqlCommandBuilder cmd = null;
        string connectionStr = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TeaShop;Integrated Security=True";
        //  підключення до бази даних
       // connectionStr = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TeaShop;Integrated Security=True";
       // connect.ConnectionString = connectionStr;

        public MainWindow()
        {
            InitializeComponent();
            connect.ConnectionString = connectionStr;
        }

        private void btnFILL_Click(object sender, RoutedEventArgs e)
        {
            // визначаю яке завдання обрав користувач в comboBox
            int taskk = 0;
            string input = comboTasks.Text;
            try
            {
                string pattern = @"(\d+)_(\d+)";
                Match match = Regex.Match(input, pattern);
                if (match.Success)
                    taskk = int.Parse(match.Groups[1].Value + match.Groups[2].Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ой! Щось пішло не так з вибором завдання: " + ex.Message);
            }


            try
            {
                switch (taskk)
                {
                    case 21:
                        {   // Отображение всей информации о чаях
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT T.Id, T.NameTea, T.Country, 
                                B.NameOfBlack, 
                                G.NameOfGreen, 
                                R.NameOfRed, 
                                W.NameOfWhite, 
                                Y.NameOfYellow
                        FROM Teas T
                        LEFT JOIN Blacks B ON T.Id = B.TeaId
                        LEFT JOIN Greens G ON T.Id = G.TeaId
                        LEFT JOIN Reds R ON T.Id = R.TeaId
                        LEFT JOIN Whites W ON T.Id = W.TeaId
                        LEFT JOIN Yellows Y ON T.Id = Y.TeaId";
                            DataTable dataTable = GetTeasValue(sql_query);
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 22:
                        {   // Отображение всех названий чаёв
                            dataGrid.ItemsSource = null;
                            string sql_query = "SELECT * FROM Teas";
                            DataTable dataTable = GetTeasValue(sql_query);
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 23:
                        {   // Отображение всех зеленых чаёв
                            dataGrid.ItemsSource = null;
                            string sql_query = "SELECT * FROM Greens";
                            DataTable dataTable = GetTeasValue(sql_query);
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 24:
                        {   // Отображение всех черных чаёв
                            dataGrid.ItemsSource = null;
                            string sql_query = "SELECT * FROM Blacks";
                            DataTable dataTable = GetTeasValue(sql_query);
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 25:
                        {   // Отображение всех чаёв, кроме зеленых и чёрных
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT T.Id, T.NameTea, T.Country,                                
                                R.NameOfRed, R.Description,
                                W.NameOfWhite, W.Description,
                                Y.NameOfYellow, Y.Description
                        FROM Teas T                       
                        LEFT JOIN Reds R ON T.Id = R.TeaId
                        LEFT JOIN Whites W ON T.Id = W.TeaId
                        LEFT JOIN Yellows Y ON T.Id = Y.TeaId";
                            DataTable dataTable = GetTeasValue(sql_query);
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 26:
                        {   // Показать название всех чаёв, которых в наличие не больше, чем количество грамм, указанных пользователем.

                            int quantityGrm = 1000;     // припустимо що користувач задав 1000 грам.
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT T.NameTea, B.NameOfBlack, G.NameOfGreen, R.NameOfRed, W.NameOfWhite, Y.NameOfYellow
                             FROM Teas T
                             LEFT JOIN Blacks B ON T.Id = B.TeaId
                             LEFT JOIN Greens G ON T.Id = G.TeaId
                             LEFT JOIN Reds R ON T.Id = R.TeaId
                             LEFT JOIN Whites W ON T.Id = W.TeaId
                             LEFT JOIN Yellows Y ON T.Id = Y.TeaId
                             WHERE B.Quantity_grm <= @QuantityGrm OR
                                   G.Quantity_grm <= @QuantityGrm OR
                                   R.Quantity_grm <= @QuantityGrm OR
                                   W.Quantity_grm <= @QuantityGrm OR
                                   Y.Quantity_grm <= @QuantityGrm";
                            DataTable availableTeas = GetAvailableTeas(quantityGrm, sql_query);
                            dataGrid.ItemsSource = availableTeas.DefaultView;
                            break;
                        }
                    case 27:
                        {   // Показать минимальную себестоимость чая
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT MIN(PrimeCost) AS MinPrimeCost
                             FROM
                             (
                                 SELECT PrimeCost FROM Blacks
                                 UNION ALL
                                 SELECT PrimeCost FROM Greens
                                 UNION ALL
                                 SELECT PrimeCost FROM Reds
                                 UNION ALL
                                 SELECT PrimeCost FROM Whites
                                 UNION ALL
                                 SELECT PrimeCost FROM Yellows
                             ) AS AllTeas";
                            decimal minPrimeCost = GetResult(sql_query, "MinPrimeCost");
                            MessageBox.Show("Мінімальна собівартість чаю: " + minPrimeCost.ToString() + "грн/грам");
                            break;
                        }
                    case 28:
                        {   // Показать максимальную себестоимость чая
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT MAX(PrimeCost) AS MaxPrimeCost
                             FROM
                             (
                                 SELECT PrimeCost FROM Blacks
                                 UNION ALL
                                 SELECT PrimeCost FROM Greens
                                 UNION ALL
                                 SELECT PrimeCost FROM Reds
                                 UNION ALL
                                 SELECT PrimeCost FROM Whites
                                 UNION ALL
                                 SELECT PrimeCost FROM Yellows
                             ) AS AllTeas";
                            decimal maxPrimeCost = GetResult(sql_query, "MaxPrimeCost");
                            MessageBox.Show("Максимальна собівартість чаю: " + maxPrimeCost.ToString() + "грн/грам");
                            break;
                        }
                    case 29:
                        {   // Показать среднюю себестоимость чая
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT AVG(PrimeCost) AS AveragePrimeCost
                             FROM
                             (
                                 SELECT PrimeCost FROM Blacks
                                 UNION ALL
                                 SELECT PrimeCost FROM Greens
                                 UNION ALL
                                 SELECT PrimeCost FROM Reds
                                 UNION ALL
                                 SELECT PrimeCost FROM Whites
                                 UNION ALL
                                 SELECT PrimeCost FROM Yellows
                             ) AS AllTeas";
                            decimal averagePrimeCost = GetResult(sql_query, "AveragePrimeCost");
                            MessageBox.Show("Середня собівартість чаю: " + averagePrimeCost.ToString());
                            break;
                        }
                    case 210:
                        {   // Показать количество чаёв, у которых себестоимость равна минимальной себестоимости
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT COUNT(*) AS TeaCount
                             FROM
                             (
                                 SELECT PrimeCost FROM Blacks
                                 UNION ALL
                                 SELECT PrimeCost FROM Greens
                                 UNION ALL
                                 SELECT PrimeCost FROM Reds
                                 UNION ALL
                                 SELECT PrimeCost FROM Whites
                                 UNION ALL
                                 SELECT PrimeCost FROM Yellows
                             ) AS AllTeas
                             WHERE PrimeCost = (SELECT MIN(PrimeCost) FROM
                                 (
                                     SELECT PrimeCost FROM Blacks
                                     UNION ALL
                                     SELECT PrimeCost FROM Greens
                                     UNION ALL
                                     SELECT PrimeCost FROM Reds
                                     UNION ALL
                                     SELECT PrimeCost FROM Whites
                                     UNION ALL
                                     SELECT PrimeCost FROM Yellows
                                 ) AS AllTeas)";
                            decimal teaCount = GetResult(sql_query, "TeaCount");
                            MessageBox.Show("Кількість чаю з мінімальною собівартістю: " + teaCount.ToString());
                            break;
                        }
                    case 211:
                        {   // Показать количество чаёв, у которых себестоимость равна максимальной себестоимости
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT COUNT(*) AS TeaCount
                            FROM
                            (
                                SELECT PrimeCost FROM Blacks
                                UNION ALL
                                SELECT PrimeCost FROM Greens
                                UNION ALL
                                SELECT PrimeCost FROM Reds
                                UNION ALL
                                SELECT PrimeCost FROM Whites
                                UNION ALL
                                SELECT PrimeCost FROM Yellows
                            ) AS AllTeas
                            WHERE PrimeCost = (SELECT MAX(PrimeCost) FROM
                            (
                                SELECT PrimeCost FROM Blacks
                                UNION ALL
                                SELECT PrimeCost FROM Greens
                                UNION ALL
                                SELECT PrimeCost FROM Reds
                                UNION ALL
                                SELECT PrimeCost FROM Whites
                                UNION ALL
                                SELECT PrimeCost FROM Yellows
                            ) AS AllTeas)";
                            decimal teaCount = GetResult(sql_query, "TeaCount");
                            MessageBox.Show("Кількість чаю з максимальною собівартістю: " + teaCount.ToString());
                            break;
                        }
                    case 212:
                        {   // Показать количество чаёв, у которых себестоимость больше средней себестоимости
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT COUNT(*) AS TeaCount
                            FROM
                            (
                                SELECT PrimeCost FROM Blacks
                                UNION ALL
                                SELECT PrimeCost FROM Greens
                                UNION ALL
                                SELECT PrimeCost FROM Reds
                                UNION ALL
                                SELECT PrimeCost FROM Whites
                                UNION ALL
                                SELECT PrimeCost FROM Yellows
                            ) AS AllTeas
                            WHERE PrimeCost > (SELECT AVG(PrimeCost) FROM
                            (
                                SELECT PrimeCost FROM Blacks
                                UNION ALL
                                SELECT PrimeCost FROM Greens
                                UNION ALL
                                SELECT PrimeCost FROM Reds
                                UNION ALL
                                SELECT PrimeCost FROM Whites
                                UNION ALL
                                SELECT PrimeCost FROM Yellows
                            ) AS AllTeas)";
                            decimal teaCount = GetResult(sql_query, "TeaCount");
                            MessageBox.Show("Кількість чаю з собівартістб вище за середню: " + teaCount.ToString());
                            break;
                        }
                    case 213:
                        {   // Показать количество единиц каждого вида чая
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT 'Blacks' AS TeaType, COUNT(*) AS TeaCount FROM Blacks
                            UNION ALL
                            SELECT 'Greens' AS TeaType, COUNT(*) AS TeaCount FROM Greens
                            UNION ALL
                            SELECT 'Reds' AS TeaType, COUNT(*) AS TeaCount FROM Reds
                            UNION ALL
                            SELECT 'Whites' AS TeaType, COUNT(*) AS TeaCount FROM Whites
                            UNION ALL
                            SELECT 'Yellows' AS TeaType, COUNT(*) AS TeaCount FROM Yellows";
                            DataTable dataTable = GetTeasValue(sql_query);
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 31:
                        {   // Показать информацию о чае в описании, которого встречается упоминание вишни
                            dataGrid.ItemsSource = null;
                            DataTable dataTable = new DataTable();
                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string sql_query = @"SELECT *
                                FROM
                                (
                                    SELECT Id, NameOfBlack AS Name, Description
                                    FROM Blacks
                                    UNION ALL
                                    SELECT Id, NameOfGreen AS Name, Description
                                    FROM Greens
                                    UNION ALL
                                    SELECT Id, NameOfRed AS Name, Description
                                    FROM Reds
                                    UNION ALL
                                    SELECT Id, NameOfWhite AS Name, Description
                                    FROM Whites
                                    UNION ALL
                                    SELECT Id, NameOfYellow AS Name, Description
                                    FROM Yellows
                                ) AS AllTeas
                                WHERE Description LIKE @SearchTerm";
                                SqlDataAdapter adapter = new SqlDataAdapter(sql_query, connection);
                                adapter.SelectCommand.Parameters.AddWithValue("@SearchTerm", "%вишня%");
                                adapter.Fill(dataTable);
                            }
                            if (dataTable.Rows.Count > 0) dataGrid.ItemsSource = dataTable.DefaultView;
                            else MessageBox.Show("Немає чаю із смаком вишні.");
                            break;
                        }
                    case 32:
                        {   // Показать информацию о чае с себестоимостью в указанном диапазоне
                            decimal minCost = 100; // Мін собівартість
                            decimal maxCost = 300; // Мах собівартість
                            dataGrid.ItemsSource = null;
                            DataTable dataTable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string query = @"SELECT *
                             FROM
                             (
                                 SELECT Id, NameOfBlack AS Name, PrimeCost
                                 FROM Blacks
                                 UNION ALL
                                 SELECT Id, NameOfGreen AS Name, PrimeCost
                                 FROM Greens
                                 UNION ALL
                                 SELECT Id, NameOfRed AS Name, PrimeCost
                                 FROM Reds
                                 UNION ALL
                                 SELECT Id, NameOfWhite AS Name, PrimeCost
                                 FROM Whites
                                 UNION ALL
                                 SELECT Id, NameOfYellow AS Name, PrimeCost
                                 FROM Yellows
                             ) AS AllTeas
                             WHERE PrimeCost >= @MinCost AND PrimeCost <= @MaxCost";

                                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                                adapter.SelectCommand.Parameters.AddWithValue("@MinCost", minCost);
                                adapter.SelectCommand.Parameters.AddWithValue("@MaxCost", maxCost);
                                adapter.Fill(dataTable);

                                if (dataTable.Rows.Count > 0)
                                    dataGrid.ItemsSource = dataTable.DefaultView;
                                else MessageBox.Show("Чай з вказаним діапазонов собівартості відсутній.");

                            }
                            break;
                        }
                    case 33:
                        {   // Показать информацию о чае с количеством грамм в указанном диапазоне
                            decimal minWeightt = 100; // Мін вага
                            decimal maxWeight = 1000; // Мах вага
                            dataGrid.ItemsSource = null;
                            DataTable dataTable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string query = @"SELECT *
                             FROM
                             (
                                 SELECT Id, NameOfBlack AS Name, Quantity_grm
                                 FROM Blacks
                                 UNION ALL
                                 SELECT Id, NameOfGreen AS Name, Quantity_grm
                                 FROM Greens
                                 UNION ALL
                                 SELECT Id, NameOfRed AS Name, Quantity_grm
                                 FROM Reds
                                 UNION ALL
                                 SELECT Id, NameOfWhite AS Name, Quantity_grm
                                 FROM Whites
                                 UNION ALL
                                 SELECT Id, NameOfYellow AS Name, Quantity_grm
                                 FROM Yellows
                             ) AS AllTeas
                             WHERE Quantity_grm >= @MinWeightt AND Quantity_grm <= @MaxWeightt";

                                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                                adapter.SelectCommand.Parameters.AddWithValue("@MinWeightt", minWeightt);
                                adapter.SelectCommand.Parameters.AddWithValue("@MaxWeightt", maxWeight);
                                adapter.Fill(dataTable);

                                if (dataTable.Rows.Count > 0)
                                    dataGrid.ItemsSource = dataTable.DefaultView;
                                else MessageBox.Show("Чай з вказаним діапазонов ваги відсутній.");
                            }
                            break;
                        }
                    case 34:
                        {   // Показать информацию о чае из указанных стран
                            string choiceCountry = "England";
                            //string choiceCountry = "Chile";
                            //string choiceCountry = "China";
                            //string choiceCountry = "Panama";
                            //string choiceCountry = "India";
                            //string choiceCountry = "Japan";
                            dataGrid.ItemsSource = null;
                            DataTable dataTable = new DataTable();
                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string sql_query = @"SELECT T.NameTea AS Name, T.Country, B.NameOfBlack AS BlackName, 
                                                    G.NameOfGreen AS GreenName, R.NameOfRed AS RedName, 
                                                    W.NameOfWhite AS WhiteName, Y.NameOfYellow AS YellowName
                                 FROM Teas T
                                 LEFT JOIN Blacks B ON T.Id = B.TeaId
                                 LEFT JOIN Greens G ON T.Id = G.TeaId
                                 LEFT JOIN Reds R ON T.Id = R.TeaId
                                 LEFT JOIN Whites W ON T.Id = W.TeaId
                                 LEFT JOIN Yellows Y ON T.Id = Y.TeaId
                                 WHERE T.Country LIKE @TeaCountry";
                                SqlDataAdapter adapter = new SqlDataAdapter(sql_query, connection);
                                adapter.SelectCommand.Parameters.AddWithValue("@TeaCountry", choiceCountry);
                                adapter.Fill(dataTable);
                            }
                            if (dataTable.Rows.Count > 0) dataGrid.ItemsSource = dataTable.DefaultView;
                            else MessageBox.Show($"Немає чаю походженням із {choiceCountry}.");
                            break;
                        }
                    case 41:
                        {   // Отобразить название страны происхождения и количество чаёв из этой страны
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT Country, SUM(TeaCount) AS TeaCount
                             FROM
                             (                               
                                SELECT T.Country, COUNT(*) AS TeaCount
                                FROM Blacks B
                                JOIN Teas T ON T.Id = B.TeaId
                                GROUP BY T.Country
                                UNION ALL
                                SELECT T.Country, COUNT(*) AS TeaCount
                                FROM Greens G
                                JOIN Teas T ON T.Id = G.TeaId
                                GROUP BY T.Country
                                UNION ALL
                                SELECT T.Country, COUNT(*) AS TeaCount
                                FROM Reds R
                                JOIN Teas T ON T.Id = R.TeaId
                                GROUP BY T.Country
                                UNION ALL
                                SELECT T.Country, COUNT(*) AS TeaCount
                                FROM Whites W
                                JOIN Teas T ON T.Id = W.TeaId
                                GROUP BY T.Country
                                UNION ALL
                                SELECT T.Country, COUNT(*) AS TeaCount
                                FROM Yellows Y
                                JOIN Teas T ON T.Id = Y.TeaId
                                GROUP BY T.Country
                             ) AS AllTeas
                             GROUP BY Country";
                            DataTable dataTable = GetTeasValue(sql_query);
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 42:
                        {   // Отобразить среднее количество грамм чая по каждой стране
                            dataGrid.ItemsSource = null;
                            string sql_query = @"SELECT Country, AVG(Quantity_grm) AS AvgGrams
                             FROM
                             (                               
                                SELECT T.Country, B.Quantity_grm
                                FROM Blacks B
                                JOIN Teas T ON T.Id = B.TeaId
                                UNION ALL
                                SELECT T.Country, G.Quantity_grm
                                FROM Greens G
                                JOIN Teas T ON T.Id = G.TeaId
                                UNION ALL
                                SELECT T.Country, R.Quantity_grm
                                FROM Reds R
                                JOIN Teas T ON T.Id = R.TeaId
                                UNION ALL
                                SELECT T.Country, W.Quantity_grm
                                FROM Whites W
                                JOIN Teas T ON T.Id = W.TeaId
                                UNION ALL
                                SELECT T.Country, Y.Quantity_grm
                                FROM Yellows Y
                                JOIN Teas T ON T.Id = Y.TeaId
                             ) AS AllTeas
                             GROUP BY Country";
                            DataTable dataTable = GetTeasValue(sql_query);
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 43:
                        {   // Показать три самых дешевых чая по конкретной стране                            

                            //string choiceCountry = "Chile";
                            //string choiceCountry = "China";
                            //string choiceCountry = "Panama";
                            string choiceCountry = "India";
                            //string choiceCountry = "England";
                            //string choiceCountry = "Japan";
                            dataGrid.ItemsSource = null;

                            DataTable dataTable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string query = $@"SELECT TOP 3 TeaCountry, TeaName, TeaCost
                                FROM
                                (
                                    SELECT T.Country AS TeaCountry, B.NameOfBlack AS TeaName, B.PrimeCost AS TeaCost
                                    FROM Blacks B
                                    JOIN Teas T ON T.Id = B.TeaId
                                    WHERE T.Country = @Country
                                    UNION ALL
                                    SELECT T.Country, G.NameOfGreen, G.PrimeCost
                                    FROM Greens G
                                    JOIN Teas T ON T.Id = G.TeaId
                                    WHERE T.Country = @Country
                                    UNION ALL
                                    SELECT T.Country, R.NameOfRed, R.PrimeCost
                                    FROM Reds R
                                    JOIN Teas T ON T.Id = R.TeaId
                                    WHERE T.Country = @Country
                                    UNION ALL
                                    SELECT T.Country, W.NameOfWhite, W.PrimeCost
                                    FROM Whites W
                                    JOIN Teas T ON T.Id = W.TeaId
                                    WHERE T.Country = @Country
                                    UNION ALL
                                    SELECT T.Country, Y.NameOfYellow, Y.PrimeCost
                                    FROM Yellows Y
                                    JOIN Teas T ON T.Id = Y.TeaId
                                    WHERE T.Country = @Country
                                ) AS AllTeas
                                ORDER BY TeaCost";

                                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                                adapter.SelectCommand.Parameters.AddWithValue("@Country", choiceCountry);
                                adapter.Fill(dataTable);
                            }
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 44:
                        {   // Показать три самых дорогих чая по конкретной стране

                            //string choiceCountry = "Chile";
                            //string choiceCountry = "China";
                            string choiceCountry = "Panama";
                            //string choiceCountry = "India";
                            //string choiceCountry = "England";
                            //string choiceCountry = "Japan";
                            dataGrid.ItemsSource = null;

                            DataTable dataTable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string query = $@"SELECT TOP 3 TeaCountry, TeaName, TeaCost
                                FROM
                                (
                                    SELECT T.Country AS TeaCountry, B.NameOfBlack AS TeaName, B.PrimeCost AS TeaCost
                                    FROM Blacks B
                                    JOIN Teas T ON T.Id = B.TeaId
                                    WHERE T.Country = @Country
                                    UNION ALL
                                    SELECT T.Country, G.NameOfGreen, G.PrimeCost
                                    FROM Greens G
                                    JOIN Teas T ON T.Id = G.TeaId
                                    WHERE T.Country = @Country
                                    UNION ALL
                                    SELECT T.Country, R.NameOfRed, R.PrimeCost
                                    FROM Reds R
                                    JOIN Teas T ON T.Id = R.TeaId
                                    WHERE T.Country = @Country
                                    UNION ALL
                                    SELECT T.Country, W.NameOfWhite, W.PrimeCost
                                    FROM Whites W
                                    JOIN Teas T ON T.Id = W.TeaId
                                    WHERE T.Country = @Country
                                    UNION ALL
                                    SELECT T.Country, Y.NameOfYellow, Y.PrimeCost
                                    FROM Yellows Y
                                    JOIN Teas T ON T.Id = Y.TeaId
                                    WHERE T.Country = @Country
                                ) AS AllTeas
                                ORDER BY TeaCost DESC";

                                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                                adapter.SelectCommand.Parameters.AddWithValue("@Country", choiceCountry);
                                adapter.Fill(dataTable);
                            }
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 45:
                        {   // Показать три самых дешевых чая по всем странам
                            dataGrid.ItemsSource = null;

                            DataTable dataTable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string query = $@"SELECT TOP 3 TeaCountry, TeaName, TeaCost
                                FROM
                                (
                                    SELECT T.Country AS TeaCountry, B.NameOfBlack AS TeaName, B.PrimeCost AS TeaCost
                                    FROM Blacks B
                                    JOIN Teas T ON T.Id = B.TeaId
                                    UNION ALL
                                    SELECT T.Country, G.NameOfGreen, G.PrimeCost
                                    FROM Greens G
                                    JOIN Teas T ON T.Id = G.TeaId
                                    UNION ALL
                                    SELECT T.Country, R.NameOfRed, R.PrimeCost
                                    FROM Reds R
                                    JOIN Teas T ON T.Id = R.TeaId
                                    UNION ALL
                                    SELECT T.Country, W.NameOfWhite, W.PrimeCost
                                    FROM Whites W
                                    JOIN Teas T ON T.Id = W.TeaId
                                    UNION ALL
                                    SELECT T.Country, Y.NameOfYellow, Y.PrimeCost
                                    FROM Yellows Y
                                    JOIN Teas T ON T.Id = Y.TeaId
                                ) AS AllTeas
                                ORDER BY TeaCost";

                                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                                adapter.Fill(dataTable);
                            }
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 46:
                        {   // Показать три самых дорогих чая по всем странам
                            dataGrid.ItemsSource = null;

                            DataTable dataTable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string query = @"SELECT TOP 3 TeaCountry, TeaName, TeaCost
                                FROM
                                (
                                    SELECT T.Country AS TeaCountry, B.NameOfBlack AS TeaName, B.PrimeCost AS TeaCost
                                    FROM Blacks B
                                    JOIN Teas T ON T.Id = B.TeaId
                                    UNION ALL
                                    SELECT T.Country, G.NameOfGreen, G.PrimeCost
                                    FROM Greens G
                                    JOIN Teas T ON T.Id = G.TeaId
                                    UNION ALL
                                    SELECT T.Country, R.NameOfRed, R.PrimeCost
                                    FROM Reds R
                                    JOIN Teas T ON T.Id = R.TeaId
                                    UNION ALL
                                    SELECT T.Country, W.NameOfWhite, W.PrimeCost
                                    FROM Whites W
                                    JOIN Teas T ON T.Id = W.TeaId
                                    UNION ALL
                                    SELECT T.Country, Y.NameOfYellow, Y.PrimeCost
                                    FROM Yellows Y
                                    JOIN Teas T ON T.Id = Y.TeaId
                                ) AS AllTeas
                                ORDER BY TeaCost DESC";

                                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                                adapter.Fill(dataTable);
                            }
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 51:
                        {   // Показать топ-3 стран по количеству чаёв
                            dataGrid.ItemsSource = null;

                            DataTable dataTable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string query = $@"SELECT TOP 3 TeaCountry, TeaCount
                                 FROM
                                 (
                                     SELECT TeaCountry, COUNT(*) AS TeaCount
                                     FROM
                                     (
                                         SELECT T.Country AS TeaCountry, B.TeaId
                                         FROM Blacks B
                                         JOIN Teas T ON T.Id = B.TeaId
                                         UNION ALL
                                         SELECT T.Country, G.TeaId
                                         FROM Greens G
                                         JOIN Teas T ON T.Id = G.TeaId
                                         UNION ALL
                                         SELECT T.Country, R.TeaId
                                         FROM Reds R
                                         JOIN Teas T ON T.Id = R.TeaId
                                         UNION ALL
                                         SELECT T.Country, W.TeaId
                                         FROM Whites W
                                         JOIN Teas T ON T.Id = W.TeaId
                                         UNION ALL
                                         SELECT T.Country, Y.TeaId
                                         FROM Yellows Y
                                         JOIN Teas T ON T.Id = Y.TeaId
                                     ) AS AllTeas
                                     GROUP BY TeaCountry, TeaId
                                 ) AS GroupedTeas
                                 GROUP BY TeaCountry, TeaCount
                                 ORDER BY TeaCount DESC";

                                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                                adapter.Fill(dataTable);
                            }
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 52:
                        {   // Показать топ-3 стран по количеству грамм чая в наличии
                            dataGrid.ItemsSource = null;

                            DataTable dataTable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string query = $@"SELECT TOP 3 TeaCountry, SUM(Quantity_grm) AS TotalQuantity
                                FROM
                                (
                                    SELECT T.Country AS TeaCountry, B.Quantity_grm
                                    FROM Blacks B
                                    JOIN Teas T ON T.Id = B.TeaId
                                    UNION ALL
                                    SELECT T.Country, G.Quantity_grm
                                    FROM Greens G
                                    JOIN Teas T ON T.Id = G.TeaId
                                    UNION ALL
                                    SELECT T.Country, R.Quantity_grm
                                    FROM Reds R
                                    JOIN Teas T ON T.Id = R.TeaId
                                    UNION ALL
                                    SELECT T.Country, W.Quantity_grm
                                    FROM Whites W
                                    JOIN Teas T ON T.Id = W.TeaId
                                    UNION ALL
                                    SELECT T.Country, Y.Quantity_grm
                                    FROM Yellows Y
                                    JOIN Teas T ON T.Id = Y.TeaId
                                ) AS AllTeas
                                GROUP BY TeaCountry
                                ORDER BY TotalQuantity DESC";

                                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                                adapter.Fill(dataTable);
                            }
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 53:
                        {   // Показать топ-3 зелёных чаёв по количеству грамм
                            dataGrid.ItemsSource = null;

                            DataTable dataTable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string query = $@"SELECT TOP 3 NameOfGreen, SUM(Quantity_grm) AS TotalQuantity
                                FROM Greens                               
                                GROUP BY NameOfGreen
                                ORDER BY TotalQuantity DESC";

                                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                                adapter.Fill(dataTable);
                            }
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 54:
                        {   // Показать топ-3 чёрных чаёв по количеству грамм
                            dataGrid.ItemsSource = null;

                            DataTable dataTable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string query = $@"SELECT TOP 3 NameOfBlack, SUM(Quantity_grm) AS TotalQuantity
                                FROM Blacks                               
                                GROUP BY NameOfBlack
                                ORDER BY TotalQuantity DESC";

                                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                                adapter.Fill(dataTable);
                            }
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 55:
                        {   // Показать топ-3 чая по каждому виду по количеству грамм
                            dataGrid.ItemsSource = null;

                            DataTable dataTable = new DataTable();

                            using (SqlConnection connection = new SqlConnection(connectionStr))
                            {
                                string query = $@"SELECT TeaType, TeaName, TotalQuantity
                                FROM
                                (
                                    SELECT 'Blacks' AS TeaType, NameOfBlack AS TeaName, Quantity_grm AS TotalQuantity
                                    FROM Blacks
                                    ORDER BY Quantity_grm DESC
                                    OFFSET 0 ROWS FETCH NEXT 3 ROWS ONLY
                                    UNION ALL
                                    SELECT 'Greens', NameOfGreen, Quantity_grm
                                    FROM Greens
                                    ORDER BY Quantity_grm DESC
                                    OFFSET 0 ROWS FETCH NEXT 3 ROWS ONLY
                                    UNION ALL
                                    SELECT 'Reds', NameOfRed, Quantity_grm
                                    FROM Reds
                                    ORDER BY Quantity_grm DESC
                                    OFFSET 0 ROWS FETCH NEXT 3 ROWS ONLY
                                    UNION ALL
                                    SELECT 'Whites', NameOfWhite, Quantity_grm
                                    FROM Whites
                                    ORDER BY Quantity_grm DESC
                                    OFFSET 0 ROWS FETCH NEXT 3 ROWS ONLY
                                    UNION ALL
                                    SELECT 'Yellows', NameOfYellow, Quantity_grm
                                    FROM Yellows
                                    ORDER BY Quantity_grm DESC
                                    OFFSET 0 ROWS FETCH NEXT 3 ROWS ONLY
                                ) AS AllTeas
                                ORDER BY TeaType, TotalQuantity DESC";

                                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                                adapter.Fill(dataTable);
                            }
                            dataGrid.ItemsSource = dataTable.DefaultView;
                            break;
                        }
                    case 61:
                        {   // Добавление данных
                            dataGrid.ItemsSource = null;
                            try
                            {
                                using (SqlConnection connection = new SqlConnection(connectionStr))
                                {
                                    string query = @"SELECT *  FROM Teas";
                                    set = new DataSet();
                                    adapt = new SqlDataAdapter(query, connection);
                                    cmd = new SqlCommandBuilder(adapt);
                                    adapt.Fill(set);
                                    set.Tables[0].TableName = "Teas";
                                    DataView Source = new DataView(set.Tables["Teas"]);
                                    dataGrid.Items.Refresh();
                                    dataGrid.ItemsSource = Source;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }                            
                            break;
                        }
                    case 62:
                        {   // Редактирование данных
                            dataGrid.ItemsSource = null;
                            try
                            {
                                using (SqlConnection connection = new SqlConnection(connectionStr))
                                {
                                    string query = @"SELECT *  FROM Teas";
                                    set = new DataSet();
                                    adapt = new SqlDataAdapter(query, connection);
                                    cmd = new SqlCommandBuilder(adapt);
                                    adapt.Fill(set);
                                    set.Tables[0].TableName = "Teas";
                                    DataView Source = new DataView(set.Tables["Teas"]);
                                    dataGrid.Items.Refresh();
                                    dataGrid.ItemsSource = Source;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            break;
                        }
                    case 63:
                        {   // Удаление данных
                            dataGrid.ItemsSource = null;
                            try
                            {
                                using (SqlConnection connection = new SqlConnection(connectionStr))
                                {
                                    string query = @"SELECT *  FROM Teas";
                                    set = new DataSet();
                                    adapt = new SqlDataAdapter(query, connection);
                                    cmd = new SqlCommandBuilder(adapt);
                                    adapt.Fill(set);
                                    set.Tables[0].TableName = "Teas";
                                    DataView Source = new DataView(set.Tables["Teas"]);
                                    dataGrid.Items.Refresh();
                                    dataGrid.ItemsSource = Source;
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message);
                            }
                            break;
                        }
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message); // Виведення повідомлення про помилку у вікні MessageBox
            }
        }

        /// ///////////////////////////////////////////////////////////////////////////////////

        private DataTable GetTeasValue(string query)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionStr))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                //connection.Open();
                adapter.Fill(dataTable);
            }
            return dataTable;
        }

        private DataTable GetAvailableTeas(int quantityGrm, string query)
        {
            DataTable dataTable = new DataTable();

            using (SqlConnection connection = new SqlConnection(connectionStr))
            {
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.SelectCommand.Parameters.AddWithValue("@QuantityGrm", quantityGrm);
                adapter.Fill(dataTable);
            }
            return dataTable;
        }

        /// /////////////////////////////////////////////////////////////////////

        private decimal GetResult(string query, string table)
        {
            decimal minPrimeCost = decimal.MaxValue;

            using (SqlConnection connection = new SqlConnection(connectionStr))
            {

                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);

                DataTable dataTable = new DataTable();
                adapter.Fill(dataTable);

                if (dataTable.Rows.Count > 0)
                {
                    minPrimeCost = Convert.ToDecimal(dataTable.Rows[0][table]);
                }
            }
            return minPrimeCost;
        }

        /// ////////////////////////////////////////////////////////////////////


        private void btnUpDATE_Click(object sender, RoutedEventArgs e)
        {
            int taskk = 0;
            string input = comboTasks.Text;
            try
            {
                string pattern = @"(\d+)_(\d+)";
                Match match = Regex.Match(input, pattern);
                if (match.Success)
                    taskk = int.Parse(match.Groups[1].Value + match.Groups[2].Value);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ой! Щось пішло не так з вибором завдання: " + ex.Message);
            }

                switch (taskk)
                {
                case 61:
                    {
                        InsUpdateDelete();
                        break;
                    }
                case 62:
                    {
                        InsUpdateDelete();
                        break;
                    }
                case 63:
                    {
                        InsUpdateDelete();
                        break;
                    }
            }
 
        }

        private void InsUpdateDelete()
        {
            try
            {
                SqlCommand cmd_up = new SqlCommand("UPDATE Teas SET NameTea=@Name WHERE   Country=@Cntry", connect);

                cmd_up.Parameters.Add("@Name", SqlDbType.NVarChar);
                cmd_up.Parameters["@Name"].SourceColumn = "NameTea";
                cmd_up.Parameters[0].SourceVersion = DataRowVersion.Current;

                cmd_up.Parameters.Add("@Cntry", SqlDbType.NVarChar);
                cmd_up.Parameters["@Cntry"].SourceColumn = "Country";
                cmd_up.Parameters[1].SourceVersion = DataRowVersion.Original;
                cmd.DataAdapter.UpdateCommand = cmd_up;
                adapt.UpdateCommand = cmd_up;



                SqlCommand cmd_ins = new SqlCommand("INSERT INTO  Teas VALUES (@Name, @Cntry)", connect);

                cmd_ins.Parameters.Add("@Name", SqlDbType.NVarChar);
                cmd_ins.Parameters["@Name"].SourceColumn = "NameTea";
                cmd_ins.Parameters[0].SourceVersion = DataRowVersion.Current;

                cmd_ins.Parameters.Add("@Cntry", SqlDbType.NVarChar);
                cmd_ins.Parameters["@Cntry"].SourceColumn = "Country";
                cmd_ins.Parameters[1].SourceVersion = DataRowVersion.Current;
                cmd.DataAdapter.InsertCommand = cmd_ins;
                adapt.InsertCommand = cmd_ins;



                SqlCommand cmd_del = new SqlCommand("DELETE FROM  Teas WHERE NameTea=@Name", connect);

                cmd_del.Parameters.Add("@Name", SqlDbType.NVarChar);
                cmd_del.Parameters["@Name"].SourceColumn = "NameTea";
                cmd_del.Parameters[0].SourceVersion = DataRowVersion.Original;
                cmd.DataAdapter.DeleteCommand = cmd_del;
                adapt.DeleteCommand = cmd_del;
                adapt.Update(set, "Teas");

                MessageBox.Show("Дані успішно змінено.");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
