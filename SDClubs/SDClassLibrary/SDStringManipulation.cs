using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace SDClassLibrary
{
    public static class SDStringManipulation
    {
        public static string SDExtractDigits(string input)
        {
            string inputChars = "";
            foreach (char letter in input)
            {
                if(char.IsDigit(letter))
                {
                    inputChars += letter;
                }
            }
            return inputChars;
        }

        public static bool SDPostalCodeIsValid(string postal, string regex)
        {
            Regex rx = new Regex(regex);
            if(rx.IsMatch(postal))
            {
                return true;
            }
            else
            {
                return false;
            }    
        }

        public static string SDCapitalize(string input)
        {
            if(input == "" || input == null)
            {
                return "";
            }
            else
            {
                string inputLower = input.ToLower();
                string inputTrim = inputLower.Trim();
                string output = char.ToUpper(inputTrim[0]) + inputTrim.Substring(1);
                return output;
            }
        }
    }
}
