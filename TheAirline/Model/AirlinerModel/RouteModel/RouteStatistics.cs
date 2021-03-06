﻿
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using TheAirline.Model.GeneralModel;
using TheAirline.Model.GeneralModel.StatisticsModel;

namespace TheAirline.Model.AirlinerModel.RouteModel
{

    /*! Route statistics.
   * This is used for statistics for a route.
   * The class needs no parameters
   */
     [Serializable]
    public class RouteStatistics : ISerializable
    {
         [Versioning("stats")]
        public List<RouteStatisticsItem> Stats { get; set; }
        public RouteStatistics()
        {
            this.Stats = new List<RouteStatisticsItem>();
        }
        //clears the list
        public void clear()
        {
            this.Stats.Clear();
        }
         //returns the value for a statistics type for a route class
        public long getStatisticsValue(RouteAirlinerClass aClass, StatisticsType type)
        {
            RouteStatisticsItem item = this.Stats.Find(i => i.Type.Shortname == type.Shortname && i.RouteClass.Type == aClass.Type);

            if (item == null)
                return 0;
            else
                return item.Value;
        }
         public long getStatisticsValue(StatisticsType type)
        {
            RouteAirlinerClass aClass = new RouteAirlinerClass(AirlinerClass.ClassType.Economy_Class, RouteAirlinerClass.SeatingType.Free_Seating, 0);

            return getStatisticsValue(aClass, type);
        }
        //adds the value for a statistics type to a route class
         public void addStatisticsValue(RouteAirlinerClass aClass, StatisticsType type, int value)
         {
             RouteStatisticsItem item = this.Stats.Find(i => i.Type.Shortname == type.Shortname && i.RouteClass == aClass);

             if (item == null)
                 this.Stats.Add(new RouteStatisticsItem(aClass, type, value));
             else
                 item.Value += value;
         }
         public void addStatisticsValue(StatisticsType type, int value)
         {
            RouteAirlinerClass aClass = new RouteAirlinerClass(AirlinerClass.ClassType.Economy_Class, RouteAirlinerClass.SeatingType.Free_Seating, 0);

            addStatisticsValue(aClass,type,value);
         }
        //sets the value for a statistics type to a route class
         public void setStatisticsValue(RouteAirlinerClass aClass, StatisticsType type, int value)
         {
             RouteStatisticsItem item = this.Stats.Find(i => i.Type.Shortname == type.Shortname && i.RouteClass == aClass);

             if (item == null)
                 this.Stats.Add(new RouteStatisticsItem(aClass, type, value));
             else
                 item.Value = value;
         }
         public void setStatisticsValue(StatisticsType type, int value)
        {
            RouteAirlinerClass aClass = new RouteAirlinerClass(AirlinerClass.ClassType.Economy_Class,RouteAirlinerClass.SeatingType.Free_Seating, 0);

            setStatisticsValue(aClass, type, value);
        }
        //returns the total value of a statistics type
         public long getTotalValue(StatisticsType type)
         {
             long value = 0;

             lock (this.Stats)
             {
                 value = this.Stats.Where(s => s.Type.Shortname == type.Shortname).Sum(s => s.Value);
             }

             return value;
         }
            private RouteStatistics(SerializationInfo info, StreamingContext ctxt)
        {
            int version = info.GetInt16("version");

            var fields = this.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (SerializationEntry entry in info)
            {
                MemberInfo prop = propsAndFields.FirstOrDefault(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Name == entry.Name);


                if (prop != null)
                {
                    if (prop is FieldInfo)
                        ((FieldInfo)prop).SetValue(this, entry.Value);
                    else
                        ((PropertyInfo)prop).SetValue(this, entry.Value);
                }
            }

            var notSetProps = propsAndFields.Where(p => ((Versioning)p.GetCustomAttribute(typeof(Versioning))).Version > version);

            foreach (MemberInfo notSet in notSetProps)
            {
                Versioning ver = (Versioning)notSet.GetCustomAttribute(typeof(Versioning));

                if (ver.AutoGenerated)
                {
                    if (notSet is FieldInfo)
                        ((FieldInfo)notSet).SetValue(this, ver.DefaultValue);
                    else
                        ((PropertyInfo)notSet).SetValue(this, ver.DefaultValue);

                }

            }
        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 1);

            Type myType = this.GetType();

            var fields = myType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null);

            IList<PropertyInfo> props = new List<PropertyInfo>(myType.GetProperties(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).Where(p => p.GetCustomAttribute(typeof(Versioning)) != null));

            var propsAndFields = props.Cast<MemberInfo>().Union(fields.Cast<MemberInfo>());

            foreach (MemberInfo member in propsAndFields)
            {
                object propValue;

                if (member is FieldInfo)
                    propValue = ((FieldInfo)member).GetValue(this);
                else
                    propValue = ((PropertyInfo)member).GetValue(this, null);

                Versioning att = (Versioning)member.GetCustomAttribute(typeof(Versioning));

                info.AddValue(att.Name, propValue);
            }

        }
    }
    /*
    public class RouteStatistics
    {
        
        private Dictionary<RouteAirlinerClass, Dictionary<StatisticsType, int>> Stats;

        public RouteStatistics()
        {
            this.Stats = new Dictionary<RouteAirlinerClass, Dictionary<StatisticsType, int>>();

        }
        //clears the statistics
        public void clear()
        {
            this.Stats = new Dictionary<RouteAirlinerClass, Dictionary<StatisticsType, int>>();
        }
        //returns the value for a statistics type for a route class
        public int getStatisticsValue(RouteAirlinerClass aClass, StatisticsType type)
        {
            if (this.Stats.ContainsKey(aClass) && this.Stats[aClass].ContainsKey(type))
                return this.Stats[aClass][type];
            else
                return 0;
        }
        public int getStatisticsValue(StatisticsType type)
        {
            RouteAirlinerClass aClass = new RouteAirlinerClass(AirlinerClass.ClassType.Economy_Class, RouteAirlinerClass.SeatingType.Free_Seating, 0);

            return getStatisticsValue(aClass, type);
        }
        //adds the value for a statistics type to a route class
        public void addStatisticsValue(RouteAirlinerClass aClass, StatisticsType type, int value)
        {
            lock (this.Stats)
            {
                if (!this.Stats.ContainsKey(aClass))
                    this.Stats.Add(aClass, new Dictionary<StatisticsType, int>());
                if (!this.Stats[aClass].ContainsKey(type))
                    this.Stats[aClass].Add(type, 0);
                this.Stats[aClass][type] += value;
            }
        }
        public void addStatisticsValue(StatisticsType type, int value)
        {
            RouteAirlinerClass aClass = new RouteAirlinerClass(AirlinerClass.ClassType.Economy_Class, RouteAirlinerClass.SeatingType.Free_Seating, 0);

            addStatisticsValue(aClass,type,value);
        }
        //sets the value for a statistics type to a route class
        public void setStatisticsValue(RouteAirlinerClass aClass, StatisticsType type, int value)
        {
            if (!this.Stats.ContainsKey(aClass))
                this.Stats.Add(aClass, new Dictionary<StatisticsType, int>());
            if (!this.Stats[aClass].ContainsKey(type))
                this.Stats[aClass].Add(type, value);
            else
                this.Stats[aClass][type] = value;
        }
        public void setStatisticsValue(StatisticsType type, int value)
        {
            RouteAirlinerClass aClass = new RouteAirlinerClass(AirlinerClass.ClassType.Economy_Class,RouteAirlinerClass.SeatingType.Free_Seating, 0);

            setStatisticsValue(aClass, type, value);
        }
        //returns the total value of a statistics type
        public int getTotalValue(StatisticsType type)
        {
            int value = 0;

            lock (this.Stats)
            {
                foreach (RouteAirlinerClass aClass in this.Stats.Keys)
                {
                    if (this.Stats[aClass].ContainsKey(type))
                        value += this.Stats[aClass][type];
                }
            }

            return value;
        }
    }
     * */
    //the statistics item for route
    [Serializable]
    public class RouteStatisticsItem
    {
        public RouteAirlinerClass RouteClass { get; set; }
        public StatisticsType Type { get; set; }
        public long Value { get; set; }
        public RouteStatisticsItem(RouteAirlinerClass routeClass, StatisticsType type, int value)
        {
            this.RouteClass = routeClass;
            this.Type = type;
            this.Value = value;
        }
    }
}
