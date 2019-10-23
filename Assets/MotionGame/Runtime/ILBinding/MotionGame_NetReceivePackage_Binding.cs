using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;

using ILRuntime.CLR.TypeSystem;
using ILRuntime.CLR.Method;
using ILRuntime.Runtime.Enviorment;
using ILRuntime.Runtime.Intepreter;
using ILRuntime.Runtime.Stack;
using ILRuntime.Reflection;
using ILRuntime.CLR.Utils;

namespace ILRuntime.Runtime.Generated
{
    unsafe class MotionGame_NetReceivePackage_Binding
    {
        public static void Register(ILRuntime.Runtime.Enviorment.AppDomain app)
        {
            BindingFlags flag = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            FieldInfo field;
            Type[] args;
            Type type = typeof(MotionGame.NetReceivePackage);

            field = type.GetField("Type", flag);
            app.RegisterCLRFieldGetter(field, get_Type_0);
            app.RegisterCLRFieldSetter(field, set_Type_0);
            field = type.GetField("ProtoBodyData", flag);
            app.RegisterCLRFieldGetter(field, get_ProtoBodyData_1);
            app.RegisterCLRFieldSetter(field, set_ProtoBodyData_1);


        }



        static object get_Type_0(ref object o)
        {
            return ((MotionGame.NetReceivePackage)o).Type;
        }
        static void set_Type_0(ref object o, object v)
        {
            ((MotionGame.NetReceivePackage)o).Type = (System.UInt16)v;
        }
        static object get_ProtoBodyData_1(ref object o)
        {
            return ((MotionGame.NetReceivePackage)o).ProtoBodyData;
        }
        static void set_ProtoBodyData_1(ref object o, object v)
        {
            ((MotionGame.NetReceivePackage)o).ProtoBodyData = (System.Byte[])v;
        }


    }
}
