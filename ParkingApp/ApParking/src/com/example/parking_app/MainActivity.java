package com.example.parking_app;

import java.io.IOException;

import android.os.Bundle;

import android.app.Activity;
import android.content.Intent;

import android.view.Menu;
import android.view.View;

public class MainActivity extends Activity {   
   
	
	@Override
	protected void onCreate(Bundle savedInstanceState) {
		super.onCreate(savedInstanceState);
		setContentView(R.layout.activity_main);			
	}

	@Override 
	public boolean onCreateOptionsMenu(Menu menu) {		
		getMenuInflater().inflate(R.menu.activity_main, menu);
		return true;		
	}
	
	public void callFindEmptySlots(View view){	
		Intent intent = new Intent(this, FindSlots.class);	    
	    startActivity(intent);
		
	}
	
	public void callFindMyCar(View view){	
		Intent intent = new Intent(this, FindCarbyPlate.class);	    
	    startActivity(intent);
		
	}
	
	public void exit(View view){	
		finish();
		System.exit(0);		
	}
	
	
}
