﻿using diagram.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Threading;
using System.Data.SqlClient;


namespace diagram.DynamicDiagram
{
    public class TimeBasedDynamicDiagram : ScrollViewer
    {
        #region Constructor
        public TimeBasedDynamicDiagram(double width, DataModel model)
        {
            initializeData(width, model);
            initializeGraphics();
            initializeHandler();
        }
        #endregion

        #region Properties
        private StackPanel _panel;

        private DataModel _model;       // 存储所有数据
        private List<Column> _columns;
        private ScaleColumn _scale;

        public ScaleColumn Scale
        {
            get { return _scale; }
            set { _scale = value; }
        }
        private enum _ColumnWidth : int
        { BIG = 400, MIDDLE = 300, SMALL = 200 };
        private double _width;          // ScrollViewer的宽度
        private double _colWidth;
        private int _height;

        public DataModel Model
        {
            get { return _model; }
            set { _model = value; }
        }

        public List<Column> Columns
        {
            get { return _columns; }
            set { _columns = value; }
        }
        #endregion

        #region Initialization
        private void initializeData(double width, DataModel model)
        {
            _width = width;

            _colWidth = adjustColumnWidth(width, model.DataList.Count);
            _model = model;
            _columns = new List<Column>();
            _panel = new StackPanel();

            List<List<Data>> list = new List<List<Data>>();
            for (int i = 0; i < _model.ColumnNumber; ++i)
            {
                List<Data> datalist = new List<Data>();
                list.Add(datalist);
            }
            for (int i = 0; i < _model.DataList.Count; ++i)
            {
                Data d = _model.DataList.ElementAt(i);
                if (d.DefaultColumnPos.Count == 0)
                    continue;
                for (int j = 0; j < d.DefaultColumnPos.Count; ++j)
                    list.ElementAt(d.DefaultColumnPos.ElementAt(j) - 1).Add(d);
            }

            _scale = new ScaleColumn(_colWidth);
            _panel.Children.Add(_scale);
            _height = _scale.CanvasHeight;
            for (int i = 0; i < _model.ColumnNumber; ++i)
            {
                Column c = new Column(_colWidth, _height, list.ElementAt(i), _model);
                _columns.Add(c);
                _panel.Children.Add(c);
            }
        }

        private void initializeGraphics()
        {
            this.HorizontalScrollBarVisibility = ScrollBarVisibility.Visible;
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;

            _panel.HorizontalAlignment = HorizontalAlignment.Center;
            _panel.VerticalAlignment = VerticalAlignment.Top;
            _panel.Orientation = Orientation.Horizontal;
            this.Content = _panel;
        }

        private void initializeHandler()
        {
            this.AddHandler(Column.delColumnEvent, new RoutedEventHandler(delColumn));
            this.AddHandler(Column.saveConfigEvent, new RoutedEventHandler(saveDataConfig));
        }
        #endregion

        #region Methods
        public int adjustColumnWidth(double width, int colnum)
        {
            if (width / (int)_ColumnWidth.BIG >= colnum) { return (int)_ColumnWidth.BIG; }
            else if (width / (int)_ColumnWidth.MIDDLE >= colnum) { return (int)_ColumnWidth.MIDDLE; }
            else { return (int)_ColumnWidth.SMALL; }
        }

        public void addColumn(int pos, List<Data> list)
        {
            Column c = new Column(_colWidth, _height, list, _model);
            _columns.Insert(pos - 1, c);
            _panel.Children.Insert(pos, c);
            adjustGraphics();
        }
        #endregion

        #region RoutingMethods
        private void delColumn(object sender, RoutedEventArgs args)
        {
            delEventArgs e = (delEventArgs)args;
            _columns.RemoveAt(e.index - 1);           // stackpanel中多一列ColumnScale
            _panel.Children.RemoveAt(e.index);
            adjustGraphics();
        }

        private void adjustGraphics()
        {
            int width = adjustColumnWidth(_width, _columns.Count);
            if (width != _colWidth)
            {
                foreach (Column c in _columns)
                {
                    c.adjustGraphics(width);
                    //c.drawGraphics();
                }
                _panel.Children.RemoveRange(0, _panel.Children.Count);
                //_scale.adjustGraphics(width);
                _panel.Children.Add(_scale);
                foreach (Column c in _columns)
                {
                    _panel.Children.Add(c);
                }
                _colWidth = width;
            }
        }

        public void saveDataConfig(object sender, RoutedEventArgs args)
        {
            _model.saveDataConfig(_columns);
        }
        #endregion

        #region TimeBasedGraphics
        private SqlDataAccess conn;
        private String TableName;
        private String WellID;
        private String WellBoreID;
        private ScaleData data;

        private System.Windows.Forms.Timer _timer;

        public void startDynamicDrawing()
        {
            _timer.Enabled = true;
        }

        public void startDynamicDrawing(SqlDataAccess conn,
                                        String TableName,
                                        String WellID,
                                        String WellBoreID,
                                        ScaleData data)
        {
            this.conn = conn;
            this.TableName = TableName;
            this.WellID = WellID;
            this.WellBoreID = WellBoreID;
            this.data = data;

            _timer = new System.Windows.Forms.Timer();
            _timer.Enabled = true;
            _timer.Interval = _model.Interval;
            _timer.Tick += new EventHandler(getData);
        }

        //public void getData(Time last, ScaleData data, DataSet ds)
        //{
        //    Time first = last.subtractMinutes(_model.DisplayInterval);
        //    data.FirstTime = first;

        //    int diff = last.get_diff_by_minute(first);
        //    data.getTime(last, diff);
        //    int rowCount = ds.Tables[0].Rows.Count;
        //    for (int i = 0; i < 2; ++i)
        //    {
        //        data.Depth[i] = ds.Tables[0].Rows[rowCount / 3 * (i + 1)]["DEPTH"].ToString();
        //        data.Pos[i] = rowCount / 3 * (i + 1);
        //    }

        //    _model.getData(ds);
        //}



        public void getData(object sender, EventArgs args)
        {
            DataSet dataSet = new DataSet();
            dataSet = conn.SelectDataSet("SELECT TOP 1 * FROM " + TableName + " WHERE WELLID = '" + WellID
                                            + "' AND WELLBOREID ='" + WellBoreID + "' ORDER BY TDATE DESC, TTIME DESC");

            if (dataSet.Tables[0].Rows.Count != 0)
            {

                String date = dataSet.Tables[0].Rows[0]["TDATE"].ToString();
                String time = dataSet.Tables[0].Rows[0]["TTIME"].ToString();
                // 数据截止时间
                Time last = new Time(date, time);
                Time first = last.subtractMinutes(_model.DisplayInterval);
                date = first.ToDateString();
                time = first.ToTimeString();

                dataSet.Clear();
                dataSet.Dispose();

                DataSet ds = new DataSet();

                string str = "SELECT * FROM " + TableName + " Where (TDATE = '" + date + "' And TTime >= '" + time + "') Or (TDate >'" + date + "') AND WELLID = '" + WellID + "' AND WELLBOREID ='" + WellBoreID + "' ORDER BY TDATE asc,TTIME asc";



                //ds = conn.SelectDataSet("SELECT * FROM (SELECT * FROM " + TableName
                //                        + " WHERE TDATE >= '" + date + "') AS TEMP WHERE TEMP.TTIME >= '" + time + "' ORDER BY TEMP.TDATE asc, TEMP.TTIME asc");


                ds = conn.SelectDataSet(str);

                int N = ds.Tables[0].Rows.Count;


                // 数据起始时间
                first = getDateTime(ds.Tables[0].Rows[0]["TDATE"].ToString(),
                                         ds.Tables[0].Rows[0]["TTIME"].ToString());

                data.FirstTime = first;
                // 获取两时间的差值
                int diff = last.get_diff_by_minute(first);
                data.getTime(last, diff);

                int rowCount = ds.Tables[0].Rows.Count;
                for (int i = 0; i < 2; ++i)
                {
                    data.Depth[i] = ds.Tables[0].Rows[rowCount / 3 * (i + 1)]["DEPTMEAS"].ToString();
                    data.Pos[i] = rowCount / 3 * (i + 1);
                }

                _model.getData(ds);
                ds.Clear();
                ds.Dispose();

                adjustHeader();
                foreach (Column col in this.Columns)
                {
                    col.repaint(data);
                }
                Scale.repaintScale(data);
            }
        }

        // 调整表头
        public void adjustHeader()
        {
            foreach (Column c in _columns)
            {
                ColumnHeader header = c.Header;
                foreach (ColumnHeaderData data in header.Data)
                {
                    double max = Double.Parse(data.Lblmax.Content.ToString());
                    double min = Double.Parse(data.Lblmin.Content.ToString());
                    if (max != data.Data.Max || max != data.Data.Min)
                    {
                        data.adjustLabel();
                        c.Body.repaint();
                    }
                }
            }
        }

        // 用于获取正确的时间
        private Time getDateTime(String date, String time)
        {
            String[] DATE = date.ToString().Split('/');
            String[] TIME = time.ToString().Split(':');

            Time datetime = new Time(Convert.ToInt32(DATE[0]),
                                             Convert.ToInt32(DATE[1]),
                                             Convert.ToInt32(DATE[2]),
                                             Convert.ToInt32(TIME[0]),
                                             Convert.ToInt32(TIME[1]),
                                             Convert.ToInt32(TIME[2]));
            return datetime;
        }

        private void TimerTicked(object sender, EventArgs args)
        {
            //ScaleData data = getData();
            //adjustHeader();
            //foreach (Column col in _columns)
            //{
            //    col.repaint(data);
            //}
            //_scale.repaintScale(data);
            //double showHeight = _scale.CanvasHeight * _scale.Scale;
            //if (_depth >= showHeight + _model.DEPTMEAS._min - 100)
            //{
            //    _scale.adjustScale(_depth+500);
            //    foreach (Column c in _columns)
            //    {
            //        c._scale = _scale.Scale;
            //        c.Body.repaint();
            //        c.drawGraphics();
            //    }
            //}
            //else 
            //{
            //    addGraphics(_rowCount - 20);
            //}
        }

        public void endDrawing()
        {
            _timer.Enabled = false;
        }
        #endregion

    }
}
