package com.example.parking_app;

import java.io.IOException;
import java.io.UnsupportedEncodingException;
import java.util.ArrayList;
import java.util.List;

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

import android.os.Bundle;
import android.os.StrictMode;
import android.app.Activity;
import android.content.Context;
import android.view.Menu;
import android.view.View;
import android.view.inputmethod.InputMethodManager;
import android.widget.ArrayAdapter;
import android.widget.EditText;
import android.widget.ListView;
import android.widget.TextView;

public class FindCarbyPlate extends Activity {
	
	private EditText edText;	
	private TextView txt_text;
	private TextView txt_plate;
	ArrayAdapter<String> adapter;

	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_find_carby_plate);
		
		if (android.os.Build.VERSION.SDK_INT > 9) {
		    StrictMode.ThreadPolicy policy = new StrictMode.ThreadPolicy.Builder().permitAll().build();
		    StrictMode.setThreadPolicy(policy);
		}		
		edText = (EditText) findViewById(R.id.editText1);		
		txt_text = (TextView) findViewById(R.id.textView2);	
		txt_plate = (TextView) findViewById(R.id.textView4);
	}

	@Override
	public boolean onCreateOptionsMenu(Menu menu) {
		// Inflate the menu; this adds items to the action bar if it is present.
		getMenuInflater().inflate(R.menu.activity_find_carby_plate, menu);
		return true;
	}
	
	public void FindCar(View view){
		
		txt_text.setText("Su auto se encuentra en el aparcamiento:");
		txt_plate.setText("");		
		String plate = edText.getText().toString();
		
		InputMethodManager inputMngr = (InputMethodManager)getSystemService(Context.INPUT_METHOD_SERVICE);
		inputMngr.hideSoftInputFromWindow(getCurrentFocus().getWindowToken(), InputMethodManager.HIDE_NOT_ALWAYS);
		
		if (plate.length()<5){
			txt_text.setText("Debe ingresar una placa válida");
			return;
		}		
		
		String slot = retrieveInfo(plate); 		
		if (slot!="null"){
			txt_plate.setText(slot);
		}else{
			txt_plate.setText("No se encontraron resultados");
		}
		
	}
	
	private String retrieveInfo(String plate){	
		
		HttpPost httppost = new HttpPost("http://www.eiaparking.tk/WSparking.php");
		//HttpPost httppost = new HttpPost("http://192.168.0.14/Parking/WSparking.php");
	    HttpParams httpParameters = new BasicHttpParams();	    
	    HttpConnectionParams.setConnectionTimeout(httpParameters, 5000);
	    HttpConnectionParams.setSoTimeout(httpParameters, 10000);
	    
	    List <NameValuePair> nValPair = new ArrayList<NameValuePair>(2);
	    nValPair.add(new BasicNameValuePair("action", "FindByPlate"));
	    nValPair.add(new BasicNameValuePair("plate", plate));
	    
	    HttpClient httpclient = new DefaultHttpClient(httpParameters);       
	    
		try {
			
			httppost.setEntity(new UrlEncodedFormEntity(nValPair));			
			HttpResponse response = httpclient.execute(httppost);			
			String strlots = EntityUtils.toString(response.getEntity());			
			JSONObject JSlots = new JSONObject(strlots);			
			return JSlots.getString("parking_lot");		
			
		}catch (UnsupportedEncodingException e1) {			
		
		}catch (ClientProtocolException e1) {			
			//txt_text2.setText(e1.toString()+" Hubo error en la conexion");			
		}catch (IOException e1) {		
			//txt_text2.setText(e1.toString()+" Hubo error en la comunicacion");		
		}catch (JSONException e1) {			
			//txt_text2.setText(e1.toString()+" Hubo error con el formato de la solicitud");			
		}catch (Exception e1){				
			//txt_text2.setText(e1.toString()+" Hubo error en la conexion");			
		}	
		return "null";
		
	}

}
