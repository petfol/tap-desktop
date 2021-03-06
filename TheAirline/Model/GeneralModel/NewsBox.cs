﻿
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace TheAirline.Model.GeneralModel
{
    //the class for a news box
    [Serializable]
    public class NewsBox : INotifyPropertyChanged, ISerializable
    {
        [Versioning("hasunread")]
        private Boolean _hasunreadnews;
        public Boolean HasUnreadNews
        {
            get { return _hasunreadnews; }
            set { _hasunreadnews = value; NotifyPropertyChanged("HasUnreadNews"); }
        }
        [Versioning("news")]
        private List<News> News;
        public NewsBox()
        {
            this.News = new List<News>();
  
        }
        //adds a news to the news box
        public void addNews(News news)
        {
            this.HasUnreadNews = true;
            this.News.Add(news);
        }
        //removes a news from the news box
        public void removeNews(News news)
        {
            this.News.Remove(news);
            this.HasUnreadNews = this.News.Exists(n => n.IsUnRead);
        }
        //returns all new
        public List<News> getNews()
        {
            return this.News;
        }
        //returns all news for a specific type
        public List<News> getNews(News.NewsType type)
        {
            return this.News.FindAll((delegate(News n) { return n.Type == type; }));
        }
        //returns all news for a specific period
        public List<News> getNews(DateTime fromDate, DateTime toDate)
        {
            return this.News.FindAll((delegate(News n) { return n.Date >= fromDate && n.Date <= toDate; }));
        }
        //returns all unread news
        public List<News> getUnreadNews()
        {
            return this.News.FindAll((delegate(News n) {return !n.IsRead;}));
        }
        //clears the list of news
        public void clear()
        {
            this.News.Clear();
        }
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
           private NewsBox(SerializationInfo info, StreamingContext ctxt)
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
    //the class for a news
    [Serializable]
    public class News : ISerializable
    {
        public enum NewsType { Standard_News,Airport_News, Flight_News, Fleet_News, Airline_News, Alliance_News,Airliner_News}
        [Versioning("type")]
        public NewsType Type { get; set; }
        [Versioning("actionnews",Version=2)]
        public Boolean IsActionNews { get; set; }
        [Versioning("date")]
        public DateTime Date { get; set; }
        [Versioning("subject")]
        public string Subject { get; set; }
        [Versioning("body")]
        public string Body { get; set; }
        [Versioning("isread")]
        public Boolean IsRead { get; set; }
        public Boolean IsUnRead {get{return !this.IsRead;} set{;}}
        public delegate void ActionHandler(object o);
        public event ActionHandler Action;
        [Versioning("actionobject", Version = 2)]
        public object ActionObject { get; set; }
        public News(NewsType type, DateTime date, string subject, string body,Boolean isactionnews = false)
        {
            this.Type = type;
            this.Date = date;
            this.Subject = subject;
            this.Body = body;
            this.IsRead = false;
            this.IsActionNews = isactionnews;
        }
        //executes the news
        public void executeNews()
        {
            if (this.Action != null)
            {
                this.Action(this.ActionObject);
            }
        }
           private News(SerializationInfo info, StreamingContext ctxt)
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
            if (version == 1)
            {
                this.IsActionNews = false;
            }


        }

        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("version", 2);

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
}
