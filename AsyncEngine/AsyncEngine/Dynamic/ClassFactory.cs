namespace AsyncEngine.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class ClassFactory
    {
        private int classCount;
        private Dictionary<Signature, Type> classes;
        public static readonly ClassFactory Instance = new ClassFactory();
        private ModuleBuilder module;

        private ClassFactory()
        {
            AssemblyName name = new AssemblyName("DynamicClasses");
            AssemblyBuilder builder = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            try
            {
                this.module = builder.DefineDynamicModule("Module");
            }
            finally
            {
            }
            this.classes = new Dictionary<Signature, Type>();
        }

        private Type CreateDynamicClass(DynamicProperty[] properties)
        {
            string name = "DynamicClass" + (this.classCount + 1);
            try
            {
                TypeBuilder tb = this.module.DefineType(name, TypeAttributes.Public, typeof(DynamicClass));
                FieldInfo[] fields = this.GenerateProperties(tb, properties);
                this.GenerateEquals(tb, fields);
                this.GenerateGetHashCode(tb, fields);
                Type type = tb.CreateType();
                this.classCount++;
                return type;
            }
            finally
            {
            }
        }

        private void GenerateEquals(TypeBuilder tb, FieldInfo[] fields)
        {
            ILGenerator iLGenerator = tb.DefineMethod("Equals", MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, typeof(bool), new Type[] { typeof(object) }).GetILGenerator();
            LocalBuilder local = iLGenerator.DeclareLocal(tb);
            Label label = iLGenerator.DefineLabel();
            iLGenerator.Emit(OpCodes.Ldarg_1);
            iLGenerator.Emit(OpCodes.Isinst, tb);
            iLGenerator.Emit(OpCodes.Stloc, local);
            iLGenerator.Emit(OpCodes.Ldloc, local);
            iLGenerator.Emit(OpCodes.Brtrue_S, label);
            iLGenerator.Emit(OpCodes.Ldc_I4_0);
            iLGenerator.Emit(OpCodes.Ret);
            iLGenerator.MarkLabel(label);
            foreach (FieldInfo info in fields)
            {
                Type fieldType = info.FieldType;
                Type type2 = typeof(EqualityComparer<>).MakeGenericType(new Type[] { fieldType });
                label = iLGenerator.DefineLabel();
                iLGenerator.EmitCall(OpCodes.Call, type2.GetMethod("get_Default"), null);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, info);
                iLGenerator.Emit(OpCodes.Ldloc, local);
                iLGenerator.Emit(OpCodes.Ldfld, info);
                iLGenerator.EmitCall(OpCodes.Callvirt, type2.GetMethod("Equals", new Type[] { fieldType, fieldType }), null);
                iLGenerator.Emit(OpCodes.Brtrue_S, label);
                iLGenerator.Emit(OpCodes.Ldc_I4_0);
                iLGenerator.Emit(OpCodes.Ret);
                iLGenerator.MarkLabel(label);
            }
            iLGenerator.Emit(OpCodes.Ldc_I4_1);
            iLGenerator.Emit(OpCodes.Ret);
        }

        private void GenerateGetHashCode(TypeBuilder tb, FieldInfo[] fields)
        {
            ILGenerator iLGenerator = tb.DefineMethod("GetHashCode", MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.Public, typeof(int), Type.EmptyTypes).GetILGenerator();
            iLGenerator.Emit(OpCodes.Ldc_I4_0);
            foreach (FieldInfo info in fields)
            {
                Type fieldType = info.FieldType;
                Type type2 = typeof(EqualityComparer<>).MakeGenericType(new Type[] { fieldType });
                iLGenerator.EmitCall(OpCodes.Call, type2.GetMethod("get_Default"), null);
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, info);
                iLGenerator.EmitCall(OpCodes.Callvirt, type2.GetMethod("GetHashCode", new Type[] { fieldType }), null);
                iLGenerator.Emit(OpCodes.Xor);
            }
            iLGenerator.Emit(OpCodes.Ret);
        }

        private FieldInfo[] GenerateProperties(TypeBuilder tb, DynamicProperty[] properties)
        {
            FieldInfo[] infoArray = new FieldBuilder[properties.Length];
            for (int i = 0; i < properties.Length; i++)
            {
                DynamicProperty property = properties[i];
                FieldBuilder field = tb.DefineField("_" + property.Name, property.Type, FieldAttributes.Private);
                PropertyBuilder builder2 = tb.DefineProperty(property.Name, PropertyAttributes.HasDefault, property.Type, null);
                MethodBuilder mdBuilder = tb.DefineMethod("get_" + property.Name, MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, property.Type, Type.EmptyTypes);
                ILGenerator iLGenerator = mdBuilder.GetILGenerator();
                iLGenerator.Emit(OpCodes.Ldarg_0);
                iLGenerator.Emit(OpCodes.Ldfld, field);
                iLGenerator.Emit(OpCodes.Ret);
                MethodBuilder builder4 = tb.DefineMethod("set_" + property.Name, MethodAttributes.SpecialName | MethodAttributes.HideBySig | MethodAttributes.Public, null, new Type[] { property.Type });
                ILGenerator generator2 = builder4.GetILGenerator();
                generator2.Emit(OpCodes.Ldarg_0);
                generator2.Emit(OpCodes.Ldarg_1);
                generator2.Emit(OpCodes.Stfld, field);
                generator2.Emit(OpCodes.Ret);
                builder2.SetGetMethod(mdBuilder);
                builder2.SetSetMethod(builder4);
                infoArray[i] = field;
            }
            return infoArray;
        }

        public Type GetDynamicClass(IEnumerable<DynamicProperty> properties)
        {
            Type type;
            Signature key = new Signature(properties);
            if (!this.classes.TryGetValue(key, out type))
            {
                type = this.CreateDynamicClass(key.properties);
                this.classes.Add(key, type);
            }
            return type;
        }
    }
}

