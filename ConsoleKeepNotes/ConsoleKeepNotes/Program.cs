using ConsoleKeepNotes;
using System.ComponentModel.DataAnnotations;
using System.Data.SQLite;
using System.Xml.Linq;

List<Note> notes = new List<Note>();

void FetchData() {
    using (SQLiteConnection conn = new SQLiteConnection("Data Source=localNotes.db;Version=3;")) {
        conn.Open();

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

        conn.Close();
    }
}

void RemoveNote(int id) {
    using (SQLiteConnection conn = new SQLiteConnection("Data Source=localNotes.db;Version=3;")) {
        conn.Open();

        using (SQLiteCommand cmd = new SQLiteCommand("delete from Notes where Name = @Name", conn)) {
            cmd.Parameters.AddWithValue("@Name", notes[id].Name);
            cmd.ExecuteNonQuery();
        }

        conn.Close();
    }
    notes.RemoveAt(id);
}

void AppendNote() {
    Console.WriteLine("Введите название заметки:");
    string Name = Console.ReadLine();

    Console.WriteLine("Введите контент заметки, для завершения ввода введите ~end на новой строке");
    List<string> lines = new List<string>();
    string temp;
    while(true) {
        temp = Console.ReadLine();
        if (temp == "~end") break;
        lines.Add(temp);
    }

    notes.Add(Note.Parse(Name, lines.ToArray()));

    using (SQLiteConnection conn = new SQLiteConnection("Data Source=localNotes.db;Version=3;")) {
        conn.Open();

        using (SQLiteCommand cmd = new SQLiteCommand("INSERT INTO Notes (Name, Created, Content) VALUES (@Name, @Created, @Content)", conn)) {
            cmd.Parameters.AddWithValue("@Name", Name);
            cmd.Parameters.AddWithValue("@Created", DateTime.Now);
            cmd.Parameters.AddWithValue("@Content", notes[^1].ToString());

            cmd.ExecuteNonQuery();
        }

        conn.Close();
    }
}

void loop() {
    int command = 0;
    int state = 0;

    while(state != -1) {
        Console.Clear();

        switch (state) {
            case 0:
                Console.WriteLine("Список заметок:");
                for (int i = 0; i < notes.Count; i++) Console.WriteLine($"{i + 1}. {notes[i].Name} (дата создания: {notes[i].Created.ToString()})");
                Console.WriteLine("Выберите следующее действие\n-1 выйти; 1 - добавить заметку; 2 - открыть заметку; 3 - удалить заметку:");
                state = Int32.Parse(Console.ReadLine());
                if (Math.Abs(state) != 1) {
                    Console.WriteLine("Введите номер нужной заметки:");
                    command = Int32.Parse(Console.ReadLine()) - 1;
                    if (command < 0 || command >= notes.Count) state = 0;
                }
                break;
            case 1:
                AppendNote();
                state = 0;
                break;
            case 2:
                notes[command].Print();
                Console.WriteLine("Введите любую строку для возвращения к списку заметок:");
                Console.ReadLine();
                state = 0;
                break;
            case 3:
                RemoveNote(command);
                state = 0;
                break;
            default:
                state = 0;
                break;
        }
    }
}

FetchData();
loop();