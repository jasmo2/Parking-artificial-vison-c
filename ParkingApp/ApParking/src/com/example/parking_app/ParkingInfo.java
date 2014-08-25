package com.example.parking_app;

import java.io.IOException;

import org.apache.http.HttpResponse;
import org.apache.http.client.ClientProtocolException;
import org.apache.http.client.HttpClient;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.params.BasicHttpParams;
import org.apache.http.params.HttpParams;
import org.apache.http.util.EntityUtils;
import org.json.JSONException;
import org.json.JSONObject;

import android.app.ProgressDialog;
import android.content.Context;
import android.os.AsyncTask;
import android.os.SystemClock;
import android.util.Log;
import android.widget.ArrayAdapter;

public class ParkingInfo extends AsyncTask<Void, String, Void> {
	private int c=0;
	private String h;
    private Context context;
    ProgressDialog dialog;
    
    public String geth(){
    	return h;
    }

        public ParkingInfo(Context cxt) {
            context = cxt;
            dialog = new ProgressDialog(context);
        }

        @Override
        protected void onPreExecute() {
            dialog.setTitle("Please wait");
            dialog.show();
        }

        @Override
        protected Void doInBackground(Void... unused) {            
        	this.h = "hola";
            while (c++<3){
            	SystemClock.sleep(1000); 
            	//this.retrieveInfo();
            	
            }            
            return (null);
        }

        @Override
        protected void onPostExecute(Void unused) {
            dialog.dismiss();
        }
        
        
    }
