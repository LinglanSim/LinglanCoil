using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Model.Basic
{
    public class ArrayEqual
    {
        public static bool IsEqual(int[] A,int[] B)
        {
            bool flag = true;
            if(A.Length==B.Length)
            {
                for(int i=0;i<A.Length;i++)
                {
                    if(A[i] != B[i])
                    {
                        flag = false;
                    }
                }
            }
            else flag=false;
            return flag;
        }
    }
}
