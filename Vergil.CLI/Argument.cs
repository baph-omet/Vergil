namespace Vergil.CLI {
    public struct Argument {
        public static string[] Args { get; set; }

        public string Name { get; set; }
        public string HelpDescription { get; set; }
        public string EnableMessage { get; set; }
        public string ValueName { get; set; }
        public bool Required { get; set; }

        public Argument(string flag, string helpDescription = "", string enableMessage = "", string valueName = "", bool required = false) {
            Name = flag;
            HelpDescription = helpDescription;
            if (!string.IsNullOrWhiteSpace(enableMessage)) EnableMessage = enableMessage;
            else EnableMessage = HelpDescription;
            ValueName = valueName;
            Required = required;
        }

        public override string ToString() {
            string v = string.Empty;
            if (!string.IsNullOrWhiteSpace(ValueName)) v = $"<{ValueName}> ";
            return $"--{Name} {v}- {HelpDescription}";
        }

        public bool ParseFlag() {
            try {
                if (HasArg()) {
                    if (!string.IsNullOrWhiteSpace(EnableMessage)) Console.WriteLine(EnableMessage);
                    return true;
                }
            } catch (Exception e) {
                Console.WriteLine($"Couldn't parse argument {Name}.");
                Console.WriteLine(e);
            }

            if (Required) {
                throw new ArgumentException("Missing required argument.", Name);
            }

            return false;
        }

        public T? ParseVariable<T>() {
            int argIndex = GetArgIndex();
            if (argIndex == -1) {
                if (Required) {
                    throw new ArgumentException("Missing required argument.", Name);
                }
                return default;
            }

            T? value = default;
            try {
                string variable = Args[argIndex + 1];

                if (string.IsNullOrWhiteSpace(variable) || variable[..2].Equals("--")) {
                    if (Required) {
                        throw new ArgumentException("Missing value for required argument.", Name);
                    }
                    Console.WriteLine($"Expected a variable after argument {Name}. Ignoring.");
                    return default;
                }

                if (typeof(T) != typeof(string)) value = (T)Convert.ChangeType(variable, typeof(T));
            } catch (Exception) {
                if (Required) {
                    throw new ArgumentException("Could not parse value for required argument.", Name);
                }
                Console.WriteLine($"Expected a variable of type {typeof(T).Name} after argument {Name}. Ignoring.");
                return default;
            }

            if (!string.IsNullOrWhiteSpace(EnableMessage)) Console.WriteLine(string.Format(EnableMessage, value));
            return value;
        }

        public string? ParsePath() {
            string? path = ParseVariable<string>();
            if (string.IsNullOrWhiteSpace(path)) return null;
            if (!Directory.Exists(path)) {
                if (Required) {
                    throw new ArgumentException("Expected the name of an existing directory.", Name);
                }
                Console.WriteLine($"Couldn't parse argument {Name}. Expected the name of an existing directory.");
                return null;
            }
            return path;
        }

        private int GetArgIndex() {
            string n = Name;
            string? a = Args.FirstOrDefault(x => x[0] == '-' && x.Contains(n, StringComparison.InvariantCultureIgnoreCase));
            if (string.IsNullOrWhiteSpace(a)) return -1;
            return Array.IndexOf(Args, a);
        }

        private bool HasArg() {
            return GetArgIndex() >= 0;
        }
    }
}