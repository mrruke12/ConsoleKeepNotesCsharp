using ConsoleKeepNotes;
using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;

string connectionString = "Data Source=localNotes.db;Version=3;";
List<Note> notes = new List<Note>();

using (SQLiteConnection conn = new SQLiteConnection(connectionString)) {
    conn.Open();

    string createSQL = "CREATE TABLE IF NOT EXISTS Notes (Id INTEGER PRIMARY KEY, Name TEXT, Created DATETIME, Content TEXT)";
    using (SQLiteCommand cmd = new SQLiteCommand(createSQL, conn)) {
        cmd.ExecuteNonQuery();
    }

;
    //using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Notes (Name, Created, Content) VALUES (@Name, @Created, @Content)", conn)) {
    //    cmd.Parameters.AddWithValue("@Name", a);
    //    cmd.Parameters.AddWithValue("@Created", b);
    //    cmd.Parameters.AddWithValue("@Content", c);

    //    cmd.ExecuteNonQuery();
    //}

    //using (SQLiteCommand cmd = new SQLiteCommand("SELECT COUNT(*) FROM Notes", conn)) {
    //    notes = new List<Note>(Convert.ToInt32(cmd.ExecuteScalar()));
    //}

    using (SQLiteCommand cmd = new SQLiteCommand("Select * from notes", conn)) {
        using (SQLiteDataReader reader = cmd.ExecuteReader()) {
            while (reader.Read()) {
                string name = reader["Name"].ToString();
                DateTime created = reader.GetDateTime(2);
                string[] content = reader["content"].ToString().Split("\n");
                notes.Add(Note.Parse(name, content, created));
            }
        }
    }

    //using (SQLiteCommand cmd = new SQLiteCommand("delete from Notes", conn)) {
    //    cmd.ExecuteNonQuery();
    //}

    conn.Close();
}

int curr = -1;
int action = 0;

while (action != -1) {
    Console.Clear();

    if (action == 0) {
        Console.WriteLine("Список заметок:");
        for (int i = 0; i < notes.Count; i++) {
            Console.WriteLine($"{i + 1}.{notes[i].Name} (создан {notes[i].Created.ToString()})");
        }
        Console.WriteLine("---------------------------------------");
        Console.WriteLine("Выберите действие (-1 - выход, 1 - открыть заметку, 2 - добавить заметку, 3 - удалить заметку):");
        action = Int32.Parse(Console.ReadLine());

        if (action == 2 || action == -1) continue;
        
        else if (notes.Count > 0 && action != -1) {
            Console.WriteLine("Напишите номер нужной заметки:");
            curr = Int32.Parse(Console.ReadLine())-1;
            if (action == 1) {
                notes[curr].Print();
                action = 0;
                Console.WriteLine("Введите любую строку для выхода на главное меню");
                Console.ReadLine();
            }
        } else action = 0;
    }else if (action == 2) {
        Console.WriteLine("Введите название заметки:");
        string name = Console.ReadLine();

        List<string> source = new List<string>();

        string temp = "";

        while (true) {
            temp = Console.ReadLine();
            if (temp == "~end") break;
            source.Add(temp);
        }

        notes.Add(Note.Parse(name, source.ToArray(), DateTime.Now));

        using (SQLiteConnection conn = new SQLiteConnection(connectionString)) {
            conn.Open();

            using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Notes (Name, Created, Content) VALUES (@Name, @Created, @Content)", conn)) {
                cmd.Parameters.AddWithValue("@Name", name);
                cmd.Parameters.AddWithValue("@Created", DateTime.Now);
                cmd.Parameters.AddWithValue("@Content", notes[^1].ToString());

                cmd.ExecuteNonQuery();
            }

            conn.Close();
        }

        action = 0;
    } else {
        if (notes.Count - 1 > curr || curr >= 0) {
            if (action == 1) notes[curr].Print();
            else if (action == 3) {
                Console.WriteLine("Заметка удалена! Ниже представлен исходный код заметки: используйте его для внесения изменений.");
                Console.WriteLine(notes[curr].ToString());

                using (SQLiteConnection conn = new SQLiteConnection(connectionString)) {
                    conn.Open();

                    using (SQLiteCommand cmd = new SQLiteCommand("delete from Notes where Name = @Name", conn)) {
                        cmd.Parameters.AddWithValue("@Name", notes[curr].Name);
                        cmd.ExecuteNonQuery();
                    }

                    conn.Close();
                }

                notes.RemoveAt(curr);
            }
        } 
        action = 0;
    } 
}