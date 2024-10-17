using ConsoleKeepNotes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ConsoleKeepNotes {
    public class Decorator {
        protected Excert obj;
        public Decorator? next;

        public Decorator(Excert obj) {
            this.obj = obj;
        }

        public virtual void Apply() { }
        public void Print() {
            Apply();

            if (next == null) {
                Console.Write(obj.ToString());
                Decorator.SetDefaults();
            }else {
                next.Print();
            }
        }
        public static void SetDefaults() {
            Console.ResetColor();
        }
    }

    public class TitleDecorator : Decorator {
        public TitleDecorator(Excert obj) : base(obj) { }

        public override void Apply() {
            Console.Write("\n");
            foreach (char c in obj.ToString()) Console.Write("_");
            Console.Write("\n");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.White;
        }
    }

    public class HighlightedDecorator : Decorator {
        public HighlightedDecorator(Excert obj) : base(obj) { }

        public override void Apply() {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.BackgroundColor = ConsoleColor.Yellow;
        }
    }
    
    public class QuoteDecorator : Decorator {
        public QuoteDecorator(Excert obj) : base(obj) { }

        public override void Apply() {
            Console.BackgroundColor = ConsoleColor.Cyan;
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.Write("    |");
        }
    }

    public class Excert {
        string content;
        Decorator? firstDecorator;
        Decorator? LastDecorator;

        public Excert(string content) {
            this.content = content;
            this.firstDecorator = LastDecorator = null;
        }

        public void Print() {
            if (firstDecorator != null) firstDecorator.Print();
            else Console.Write(content);
        }

        public override string ToString () {
            return content;
        }

        public void AddDecorator(Decorator decorator) {
            if (firstDecorator == null) {
                firstDecorator = LastDecorator = decorator;
            }else {
                LastDecorator.next = decorator;
                LastDecorator = decorator;
            }
        }
    }


    internal class Note {
        public string Name {
            get;
            init;
        }
        public DateTime Created {
            get;
            init;
        }
        List<Excert>?[] excerts;
        string[] source;

        public Note(string Name, DateTime Created, List<Excert>?[] excerts, string[] source) {
            this.Name = Name;
            this.Created = Created;
            this.excerts = excerts;
            this.source = source;
        }

        public void PrepareData (out string Name, out DateTime Created, out string Content) { 
            Name = this.Name;
            Created = this.Created;
            Content = this.ToString();
        }

        public override string ToString() {
            string res = "";
            for (int i = 0; i < source.Length; i++) {
                res += source[i];
                if (i < source.Length - 1) res += "\n";
            }
            return res;
        }

        public void Print() {
            Console.Clear();
            for (int i = 0; i < 50 + Name.Length; i++) Console.Write("_");
            Console.Write("\n");

            for (int i = 0; i < 25; i++) Console.Write(" ");
            Console.Write($"{Name}\n");

            for (int i = 0; i < 50 + Name.Length; i++) Console.Write("-");
            Console.Write("\n");
            foreach (var line in excerts) {
                foreach (Excert word in line) {
                    word.Print();
                }
                Console.WriteLine("");
            }
        }

        public static Note Parse(string name, string[] lines, DateTime date = default) {
            List<Excert>[] excerts = new List<Excert>[lines.Length];

            void AddExcert(char next, int last, int i, int j) {
                excerts[i].Add(new Excert(lines[i].Substring(last, j-last)));
                switch (next) {
                    case '$':
                        excerts[i][^1].AddDecorator(new TitleDecorator(excerts[i][^1]));
                        break;
                    case '@':
                        excerts[i][^1].AddDecorator(new QuoteDecorator(excerts[i][^1]));
                        break;
                    case '#':
                        excerts[i][^1].AddDecorator(new HighlightedDecorator(excerts[i][^1]));
                        break;
                    default:
                        break;
                }
            }

            for (int i = 0; i < lines.Length; i++) {
                excerts[i] = new List<Excert>();
                string line = lines[i];
                int last = 0;
                bool search = false;

                for (int j = 0; j < line.Length; j++) { 
                    if (!search && line[j] == '~' && !(j > 0 && line[j-1] == '\\')) { 
                        AddExcert('1', last, i, j);
                        j += 2;
                        last = j;
                        search = true;
                    } else if (search && line[j] == '~' && line[j - 1] != '\\') {
                        AddExcert(line[j + 1], last, i, j);
                        j += 2;
                        last = j;
                        search = false;
                    } else if (j == line.Length-1) {
                        AddExcert('1', last, i, j+1);
                    }
                }
            }

            return new Note(name, (date == default ? DateTime.Now : date), excerts, lines);
        }
    }
}


// пусть ~$...~$ - title, ~#...~# - highlight, ~@...~@ - quote - вложенности нет!!!

/*

первая строка - название заметки
~$Заголовок~$ это текстовая запись: вот, например, ~#выделенный текст~#
обычный текст бла бла бла, оп, ~#важная инфа~# и дальше обычный текст)))
~@а вот и цитатка~@
 
 */