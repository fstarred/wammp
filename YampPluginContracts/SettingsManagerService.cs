using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace YempPluginContracts
{
    public class SettingsManagerService
    {

        public bool ByteArrayToFile(byte[] bytes)
        {
            try
            {
                // Open file for reading
                System.IO.FileStream _FileStream = new System.IO.FileStream(path, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                
                // Writes a block of bytes to this stream using data from
                // a byte array.
                _FileStream.Write(bytes, 0, bytes.Length);

                // close file stream
                _FileStream.Close();

                return true;
            }
            catch (Exception _Exception)
            {
                // Error
                Console.WriteLine("Exception caught in process: {0}",
                                  _Exception.ToString());
            }

            // error occured, return false
            return false;
        }

        public byte[] ReadAllBytes()
        {
            if (File.Exists(path))
                return File.ReadAllBytes(path);
            else
                return null;
        }

        public byte[] Serialize(Object o)
        {

            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.AssemblyFormat
                = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            formatter.Serialize(stream, o);

            return stream.ToArray();
        }

        public Object BinaryDeSerialize(byte[] bytes)
        {
            MemoryStream stream = new MemoryStream(bytes);
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.AssemblyFormat

                = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple;
            formatter.Binder

                = new VersionConfigToNamespaceAssemblyObjectBinder();
            Object obj = (Object)formatter.Deserialize(stream);
            return obj;
        }

        internal sealed class VersionConfigToNamespaceAssemblyObjectBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                Type typeToDeserialize = null;
                try
                {
                    string ToAssemblyName = assemblyName.Split(',')[0];
                    Assembly[] Assemblies = AppDomain.CurrentDomain.GetAssemblies();
                    foreach (Assembly ass in Assemblies)
                    {
                        if (ass.FullName.Split(',')[0] == ToAssemblyName)
                        {
                            typeToDeserialize = ass.GetType(typeName);
                            break;
                        }
                    }
                }
                catch (System.Exception exception)
                {
                    throw exception;
                }
                return typeToDeserialize;
            }
        }



        protected string path;

        //protected PreMergeToMergedDeserializationBinder binder;

        public SettingsManagerService(string libpath)
        {
            this.path = libpath;
        }

        //public ISerializable ImportSettings<ISerializable>()
        //{
        //    ISerializable output;

        //    if (!File.Exists(path))
        //        return default(ISerializable);

        //    try
        //    {
        //        using (FileStream stream = new FileStream(path, FileMode.Open))
        //        {
        //            BinaryFormatter formatter = new BinaryFormatter();
        //            formatter.Binder = binder;

        //            // Deserialize the hashtable from the file and 
        //            // assign the reference to the local variable.
        //            output = (ISerializable)formatter.Deserialize(stream);
        //        }
        //    }
        //    catch (SerializationException e)
        //    {
        //        System.Diagnostics.Debug.WriteLine("Failed to deserialize. Reason: " + e.Message);
        //        throw;
        //    }

        //    return output;
        //}

        //public void ExportSettings(object model)
        //{
        //    using (FileStream stream = new FileStream(path, FileMode.Create))
        //    {
        //        // Construct a BinaryFormatter and use it to serialize the data to the stream.
        //        BinaryFormatter formatter = new BinaryFormatter();
        //        formatter.Binder = binder;
        //        try
        //        {
        //            formatter.Serialize(stream, model);
        //        }
        //        catch (SerializationException e)
        //        {
        //            System.Diagnostics.Debug.WriteLine("Failed to serialize. Reason: " + e.Message);
        //            throw;
        //        }
        //    }
        //}

    }
}
