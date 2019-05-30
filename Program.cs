using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace ConsoleApp1
{
    class Program
    {
        static List<string> special = new List<string> { "id" };
        static void Main(string[] args)
        {
            string json1;
            string json2;
            using (StreamReader r = new StreamReader("../../json1.json"))
            {
                json1 = r.ReadToEnd();
            }
            using (StreamReader r = new StreamReader("../../json2.json"))
            {
                json2 = r.ReadToEnd();
            }

            var ser1 = new JavaScriptSerializer();

            Stack<string> stack = new Stack<string>();
            StringBuilder diff = new StringBuilder(); 
            isIdentical(ser1.DeserializeObject(json1), ser1.DeserializeObject(json2),stack,diff);

            Console.WriteLine("Differbces betwween json1 and json 2 are:");
            Console.WriteLine(diff);
            Console.ReadKey();
        }

        static void isIdentical(Dictionary<string, object> json1, Dictionary<string, object> json2, Stack<string> stack, StringBuilder diff)
        {
            IEnumerable<string> keys1set = new HashSet<string>((json1).Keys).Except(special);
            IEnumerable<string> keys2set = new HashSet<string>((json2).Keys).Except(special);
            //keys1 intersect keys2
            IEnumerable<string> same = keys1set.Intersect(keys2set);
            //keys1/keys2
            IEnumerable<string> different1 = keys1set.Except(keys2set);
            //keys2/keys1
            IEnumerable<string> different2 = keys2set.Except(keys1set);

            foreach(string s in different1)
            {
                //save differnces
                string toAdd = getStackTrace(stack) + s + "(json1)";

                diff.AppendLine(toAdd);
            }
            foreach (string s in different2)
            {
                //save differnces
                string toAdd = getStackTrace(stack) + s + "(json2)";
            }
            foreach (string s in same)
            {
                //
                stack.Push(s);
                //

                isIdentical(json1[s], json2[s], stack, diff);
                
                //
                stack.Pop();
                //
            }


            return;
        }
        static void isIdentical(object[] json1, object[] json2, Stack<string> stack, StringBuilder diff)
        {
            if (json1.Length > 0 & json1[0] is Dictionary<string, object>)
            {
                //assumes json1arr & json2arr are Dictionary<string, object>[]
                if (((Dictionary<string, object>)json1[0]).Keys.Contains("id")){
                    //id typed objects
                    int abs = ((int)((Dictionary<string, object>)json1[0])["id"]) - ((int)((Dictionary<string, object>)json2[0])["id"]);
                    int i;
                    for (i=0; i<json1.Length & i<json2.Length; i++)
                    {
                        stack.Push("instance #" + i);
                        if ( ((int) ((Dictionary<string, object>)json1[i])["id"]) - ((int)((Dictionary<string, object>)json2[i])["id"]) != abs)
                        {
                            //save differnces    
                            string toAdd = getStackTrace(stack) + " different id's";
                            diff.AppendLine(toAdd);
                        }

                        isIdentical((Dictionary<string, object>) json1[i], (Dictionary<string, object>) json2[i], stack, diff);
                        stack.Pop();
                    }
                    if( i < json1.Length)
                    {

                        diff.AppendLine(getStackTrace(stack) + " (json1) has extra " + (json1.Length - i) + " fields");
                    }else if (i < json2.Length)
                    {

                        diff.AppendLine(getStackTrace(stack) + " (json2) has extra " + (json2.Length - i) + " fields");
                    }
                }
                else {
                    int i;
                    for (i = 0; i < json1.Length & i < json2.Length; i++)
                    {
                        stack.Push("instance #" + i);
                        isIdentical((Dictionary<string, object>) json1[i], (Dictionary<string, object>) json2[i], stack, diff);
                        stack.Pop();
                    }
                    if (i < json1.Length)
                    {

                        diff.AppendLine(getStackTrace(stack) + " (json1) has extra " + (json1.Length - i) + " fields");
                    }
                    else if (i < json2.Length)
                    {

                        diff.AppendLine(getStackTrace(stack) + " (json2) as extra " + (json2.Length - i) + "fields");
                    }
                }
            }
            else
            {
                //assumes json1arr & json2arr are values[]
                int i;
                for (i = 0; i < json1.Length & i < json2.Length; i++)
                {
                    //regular vals!
                    if ( ! equalVals(json1[i], json2[i]))
                    {
                        string toAdd = getStackTrace(stack) + " val #1 = " + ConvertStringsToString(getStrings(json1)) + ", val #2 = " + ConvertStringsToString(getStrings(json2));

                        diff.AppendLine(toAdd);
                        break;
                    }

                }
                if (i < json1.Length)
                {

                    diff.AppendLine(getStackTrace(stack) + " (json1) has extra " + (json1.Length - i) + " fields");
                }
                else if (i < json2.Length)
                {

                    diff.AppendLine(getStackTrace(stack) + " (json2) has extra " + (json2.Length - i) + "fields");
                }
            }
            return;

        }

        static void isIdentical(object json1, object json2, Stack<string> stack, StringBuilder diff)
        {
            if (json1 is object[] && json2 is object[])
            {
                object[] json1arr = (object[])json1;
                object[] json2arr = (object[])json2;
                isIdentical(json1arr, json2arr, stack, diff);
                
            }
            else if (json1 is Dictionary<string, object> && json2 is Dictionary<string, object>)
            {
                isIdentical((Dictionary<string, object>)json1,
                    (Dictionary<string, object>)json2, stack, diff);
                
            }
            else
            {
                if( ! equalVals(json1,json2))
                {
                    //save differnces
                    string toAdd = getStackTrace(stack) + " val #1 = " + json1 +", val #2 = " + json2;

                    diff.AppendLine(toAdd);
                }
            }

            return;
        }

        static string ConvertStringsToString(string[] array)
        {
            // Concatenate all the elements into a StringBuilder.
            StringBuilder builder = new StringBuilder();
            builder.Append('[');
            foreach (string value in array)
            {
                builder.Append(' ');
                builder.Append(value);
            }
            builder.Append(']');
            return builder.ToString();
        }

        static string[] getStrings(object[] objs)
        {
            string[] ret = new string[objs.Length];
            for(int i=0; i<objs.Length; i++)
            {
                object o = objs[i];
                if (o.GetType() == typeof(string))
                {
                    ret[i] = (string) o;
                }else if(o.GetType() == typeof(int))
                {
                    ret[i] = ((int)o).ToString();
                }
                else if (o.GetType() == typeof(double))
                {
                    ret[i] = ((double)o).ToString();
                }
            }
            return ret;
        }
        static bool equalVals(object o1, object o2)
        {
            if (o1 is int)
                if (o2 is int) return ((int)o1) == ((int)o2);
                else if (o2 is decimal) return ((int)o1) == ((decimal)o2);
                else return false;

            else if (o1 is decimal)
                if (o2 is int) return ((decimal)o1) == ((int)o2);
                else if (o2 is decimal) return ((decimal)o1) == ((decimal)o2);
                else return false;
            return false;
        }
        static string getStackTrace(Stack<string> stack)
        {
            string toAdd = "";
            foreach (string s in stack)
            {
                toAdd = s + '/' + toAdd;
            }
            return toAdd;
        }
    }
}
