using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace syntez.ServerApp.Utils
{
    /**
     * Утилита 
     */
    public class ReflectionUtil
    {                    
        private static HashSet<string> PrimitiveTypeNames = new HashSet<string>() {
                "String", "Boolean", "Double", "Int16", "Int32", "Int64", "UInt16", "UInt32", "UInt64" };

        private static HashSet<string> ObjectMethods = new HashSet<string>() {
                "GetHashCode", "Equals", "ToString", "GetType", "ReferenceEquals" };


        public ReflectionUtil() { }

        public static object Invoke( MethodInfo method, object target, JObject  args )
        {
            string state = "Поиск обьекта: ";
            Dictionary<string, object> pars;
            List<object> invArgs = null;
            try
            {
                pars = JsonConvert.DeserializeObject<Dictionary<string, object>>(args.ToString());
                invArgs = new List<object>();
                foreach (ParameterInfo pinfo in method.GetParameters())
                {
                    if( pinfo.IsOptional == false && pars.ContainsKey(pinfo.Name) == false )
                    {
                        throw new Exception("require argument "+ pinfo.Name);
                    }
                    string parameterName = pinfo.ParameterType.Name;
                    
                    if( parameterName.StartsWith("Dictionary") )
                    {                        
                        Dictionary<string, object> dictionary = JsonConvert.DeserializeObject<Dictionary<string, object>>( args[pinfo.Name].ToString() );
                        invArgs.Add( dictionary );
                    }
                    else
                    {
                        invArgs.Add( pars[pinfo.Name] );
                    }
                    
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Arguments transformation failed orr they are not valid: "+ex.Message);
            }


            try
            {                
                object result = method.Invoke(  target, invArgs.ToArray());
                state = state.Substring(0, state.Length - 7) + "успех;";
                return result;
            }
            catch( Exception ex )
            {
                throw new Exception( "Error in controller function: "+ex.Message );
            }
            
        }


        /**
         * Метод поиска обьекта 
         */
        public static Dictionary<string, Object> Find( object subject, string path  )
        {
            object p = subject;
            string[] ids = path.Split( "." );
            for ( int i = 0; i < ( ids.Length - 1 ); i++ )
            {
                string id = ids[i];
                if ( p is Dictionary<string, object> )
                {
                    p = ( ( Dictionary<string, object> ) p )[id];
                }
                else
                {
                    p = p.GetType().GetField( id ).GetValue( p );
                }
            }

            MethodInfo info = null;
            string methodName = ids[ids.Length - 1];

            foreach (var method in p.GetType().GetMethods())
            {
                if (String.Equals(methodName, method.Name))
                {
                    info = method;
                    break;
                }
            }
            Dictionary<string, Object> res = new Dictionary<string, Object>();
            res["method"] = info;
            res["target"] = p;
            res["path"] = path;


            return res;
        }



        /**
         * Метод получения семантики public-методов обьекта
         */
        public static object GetSkeleton( object subject, List<string> path )
        {

            Dictionary<string, object> actionMetadata = new Dictionary<string, object>();
            if ( subject == null || subject.GetType().IsPrimitive || PrimitiveTypeNames.Contains( subject.GetType().Name ) )
            {
                return actionMetadata;
            }
            else
            {
                if ( subject is Dictionary<string, object> )
                {
                    foreach ( var kv in ( ( Dictionary<string, object> ) subject ) )
                    {
                        actionMetadata[kv.Key] = kv.Value;
                        if ( !kv.Value.GetType().IsPrimitive && !PrimitiveTypeNames.Contains( kv.Value.GetType().Name ) )
                        {
                            
                            List<string> childPath = new List<string>( path );
                            childPath.Add( kv.Key );
                            actionMetadata[ kv.Key ] = GetSkeleton( kv.Value, childPath );
                        }
                    };
                }
                else
                {
                    //Debug.WriteLine(JObject.FromObject(subject));
                    Type type = subject.GetType();
                    //Debug.WriteLine(type.Name, path);
                    foreach ( MethodInfo info in type.GetMethods() )
                    {
                        if ( info.IsPublic && !ObjectMethods.Contains(info.Name) )
                        {
                            Dictionary<string, object> args = new Dictionary<string, object>();
                            foreach ( ParameterInfo pinfo in info.GetParameters() )
                            {
                                args[pinfo.Name] = new
                                {
                                    type = pinfo.ParameterType.Name,
                                    optional = pinfo.IsOptional,
                                    name = pinfo.Name
                                };
                            }
                            List<string> actionPath = new List<string>( path );
                            actionPath.Add( info.Name );
                            actionMetadata[info.Name] = new
                            {
                                type = "method",
                                path = actionPath,
                                args = args
                            };
                        }
                    }
                    foreach ( FieldInfo info in type.GetFields() )
                    {
                        if ( info.IsPublic )
                        {
                            if ( !info.GetType().IsPrimitive && !PrimitiveTypeNames.Contains( info.GetType().Name ) )
                            {
                                List<string> childPath = new List<string>( path );
                                childPath.Add( info.Name );
                                actionMetadata[info.Name] = GetSkeleton( info.GetValue( subject ), childPath );
                            }
                        }
                    }
                }
            }
            
            return actionMetadata;
        }
        

    }
}
