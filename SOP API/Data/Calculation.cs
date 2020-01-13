using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Vergil.Data {
    /// <summary>
    /// Specific operation types. Supports basic four functions.
    /// </summary>
    public enum Operator {
        /// <summary>
        /// Represents null, for incorrectly-formatted operators.
        /// </summary>
        NONE,
        /// <summary>
        /// Addition operator.
        /// </summary>
        ADD = '+',
        /// <summary>
        /// Subtraction operator.
        /// </summary>
        SUBTRACT = '-',
        /// <summary>
        /// Multiplication operator.
        /// </summary>
        MULTIPLY = '*',
        /// <summary>
        /// Division operator.
        /// </summary>
        DIVIDE = '/'
    }

    /// <summary>
    /// Class that represents a procedural list of operations to be performed on a value found in a DataFile.
    /// </summary>
    public class Calculation {
        /// <summary>
        /// An ordered array of Operators that indicate the order of operations for this calculation.
        /// </summary>
        public Operator[] Operations { get; set; }
        /// <summary>
        /// An ordered array of numbers that correspond to the Operations.
        /// </summary>
        public double[] Operands { get; set; }

        /// <summary>
        /// Initialize an empty Calculation. Does nothing except not be null.
        /// </summary>
        public Calculation() : this(new string[]{}) { }
        /// <summary>
        /// Initialize a new Calculation from a string array.
        /// </summary>
        /// <param name="operations">An ordered array of calculation steps, where each step is in the form oX where o is the operator and X is the operand. 
        /// i.e. "+32" or "/16". Operations will be performed in the order in which they are listed in the array. Order of operations is NOT considered.</param>
        public Calculation(string[] operations) {
            List<Operator> oprs = new List<Operator>();
            List<double> operands = new List<double>();
            foreach (string op in operations) {
                Operator o = GetOperatorFromString(op);
                if (o != Operator.NONE) {
                    try {
                        operands.Add(Convert.ToDouble(op.Substring(1)));
                        oprs.Add(o);
                    } catch (FormatException) {
                        break;
                    }
                }
            }

            Operations = oprs.ToArray();
            Operands = operands.ToArray();
        }

        /// <summary>
        /// Performs this Calculation's operations step by step on the specified value.
        /// </summary>
        /// <param name="value">The value on which to operate.</param>
        /// <returns>Returns the result of the calculation.</returns>
        public double Calculate(double value) {
            for (int i = 0; i < Operations.Length; i++) {
                switch (Operations[i]) {
                    case Operator.ADD:
                        value += Operands[i];
                        break;
                    case Operator.SUBTRACT:
                        value -= Operands[i];
                        break;
                    case Operator.MULTIPLY:
                        value *= Operands[i];
                        break;
                    case Operator.DIVIDE:
                        value /= Operands[i];
                        break;
                }
            }
            return value;
        }

        /// <summary>
        /// Gets the Operator type from the first character in the given string.
        /// </summary>
        /// <param name="op">A string whose first character contains the desired operator.</param>
        /// <returns>The operator specified in the string if found, else Operator.NONE.</returns>
        public static Operator GetOperatorFromString(string op) {
            return GetOperatorFromChar(op[0]);
        }

        /// <summary>
        /// Gets the Operator type from the given character.
        /// </summary>
        /// <param name="c">The character that represents the desired operator.</param>
        /// <returns>The operator specified in the character if found, else Operator.NONE.</returns>
        public static Operator GetOperatorFromChar(char c) {
            try {
                return (Operator) c;
            } catch (InvalidCastException) {
                return Operator.NONE;
            }
        }
    }
}
