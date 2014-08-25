package com.example.parking_app;

import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.List;

import android.os.Bundle;
import android.os.Handler;
import android.os.StrictMode;

import android.app.Activity;
import android.app.ProgressDialog;

import android.view.Menu;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.ListView;


import android.widget.TextView;

import org.apache.http.HttpResponse;
import org.apache.http.NameValuePair;

import org.apache.http.client.ClientProtocolException;
import org.apache.http.client.HttpClient;
import org.apache.http.client.entity.UrlEncodedFormEntity;
import org.apache.http.client.methods.HttpPost;
import org.apache.http.impl.client.DefaultHttpClient;
import org.apache.http.message.BasicNameValuePair;
import org.apache.http.params.BasicHttpParams;
import org.apache.http.params.HttpConnectionParams;
import org.apache.http.params.HttpParams;
import org.apache.http.util.EntityUtils;
import org.json.JSONException;
import org.json.JSONObject;

public class FindSlots extends Activity {
	
	ArrayAdapter<String> adapter;
	private int attempt;
	private Handler mHandler = new Handler();
	   
	private TextView txt_text;
	private ListView list_view;
	private TextView txt_text2;	

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_find_slots);
		
		if (android.os.Build.VERSION.SDK_INT > 9) {
		    StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder().permitAll().build();
		    StrictMode.setThreadPolicy(policy);
		}		
		txt_text = (TextView) findViewById(R.id.textView1);
		list_view = (ListView) findViewById(R.id.list_lots);
		txt_text2 = (TextView) findViewById(R.id.textView2);	
		
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.activity_find_slots, menu);
		return true;
	}
	
	/**
	 * requestInfo
	 * executed when request button is clicked and gets object PHP web service
	 * @author Julian Restrepo B	 * 
	 * @param view	 
	 * 
	 */
	public void requestInfo(View view){	
		adapter = null;
		attempt = 0;
		list_view.setAdapter(adapter);		
		txt_text.setText("Aparcamentos disponibles:");		
		txt_text2.setText("");		
		
		final ProgressDialog barProgress = ProgressDialog.show(this, "Actualizando ...","Obteniendo Información",true);
				
		final Thread th1 = new Thread (new Runnable() {			
			public void run() {		
				try {
					//Thread.sleep(4000);					
					while ( adapter == null && attempt++ < 4){						
						retrieveInfo();						
					}
				}
				catch (Exception e) {					
				}	
				barProgress.dismiss();				
			}
		});
		th1.start();
		
		Thread th2 = new Thread (new Runnable() {			
			public void run() {		
				try {										
					while (th1.getState().toString() != "TERMINATED" ){}					
					mHandler.post(new Runnable() {
                        public void run() {                        	
                        	if (attempt < 4){
                    			list_view.setAdapter(adapter);
                    		}else{
                    			txt_text2.setText("Hubo un error en la comunicación");
                    		}
                        }
                    });
				}
				catch (Exception e) {		
				}							
			}
		});
		th2.start();				
		//Log.v("th",f.getState().toString());							    	
	}	
	
	private void retrieveInfo(){		
		HttpPost httppost = new HttpPost("http://www.eiaparking.tk/WSparking.php");
		//HttpPost httppost = new HttpPost("http://192.168.0.14/Parking/WSparking.php");
	    HttpParams httpParameters = new BasicHttpParams();	    
	    HttpConnectionParams.setConnectionTimeout(httpParameters, 5000);
	    HttpConnectionParams.setSoTimeout(httpParameters, 10000);
	    //SystemClock.sleep(4000); 
	    
	    List <NameValuePair> nValPair = new ArrayList<NameValuePair>(1);
	    nValPair.add(new BasicNameValuePair("action", "FindEmptyLots"));	    
	    
	    HttpClient httpclient = new DefaultHttpClient(httpParameters);	        
	    
		try {
			httppost.setEntity(new UrlEncodedFormEntity(nValPair));	
			HttpResponse response = httpclient.execute(httppost);			
			String strlots = EntityUtils.toString(response.getEntity());			
			JSONObject JSlots = new JSONObject(strlots);			
			int size = JSlots.length();			
			String[] lots = new String[size];
			for (int i = 0;i<size;i++){	
				//lots[i] = jslots.getJSONObject(i).getString("age") + " - " + jslots.getJSONObject(i).getString("name");			
				lots[i] = (i+1) + ". " +JSlots.getString("parking_lot_"+i);			
			}				
			adapter = new ArrayAdapter<String>(this, 
	             android.R.layout.simple_list_item_1, lots);
			
		}catch (UnsupportedEncodingException e1) {	
							
		}catch (ClientProtocolException e1) {			
			//txt_text2.setText(e1.toString()+" Hubo error en la conexion");			
		}catch (IOException e1) {		
			//txt_text2.setText(e1.toString()+" Hubo error en la comunicacion");		
		}catch (JSONException e1) {			
			//txt_text2.setText(e1.toString()+" Hubo error con el formato de la solicitud");			
		}catch(Exception e1){				
			//txt_text2.setText(e1.toString()+" Hubo error en la conexion");			
		}	
		
	}

}
