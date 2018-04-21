package com.zoovisitors.pl;

import android.annotation.SuppressLint;
import android.content.Intent;
import android.content.res.Configuration;
import android.content.res.Resources;
import android.media.MediaPlayer;
import android.net.Uri;
import android.os.AsyncTask;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.util.DisplayMetrics;
import android.util.Log;
import android.widget.VideoView;

import com.google.firebase.iid.FirebaseInstanceId;
import com.zoovisitors.GlobalVariables;
import com.zoovisitors.R;
import com.zoovisitors.backend.Enclosure;
import com.zoovisitors.backend.OpeningHours;
import com.zoovisitors.bl.BusinessLayerImpl;
import com.zoovisitors.bl.FunctionInterface;
import com.zoovisitors.bl.GetObjectInterface;
import com.zoovisitors.cl.network.ResponseInterface;

import java.util.ArrayList;
import java.util.List;
import java.util.Locale;
import java.util.concurrent.CountDownLatch;

public class LoadingScreen extends BaseActivity {
    private boolean endAllThreads = false;
    private CountDownLatch doneSignal;
    private VideoView videoview;
    private List<FunctionInterface> tasks;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_loading_screen);
        //Initialize business layer (change for testing)
        GlobalVariables.appCompatActivity = this;
        GlobalVariables.bl = new BusinessLayerImpl(GlobalVariables.appCompatActivity);
        GlobalVariables.firebaseToken = FirebaseInstanceId.getInstance().getToken();
        //TODO: Delete this when sending device id to the server
        Log.e("TOKEN", "token " + GlobalVariables.firebaseToken);

        tasks = new ArrayList<>();


        videoview = (VideoView) findViewById(R.id.loading_video);
        videoview.setTranslationX(-525f);
        videoview.setOnPreparedListener(new MediaPlayer.OnPreparedListener() {
            @Override
            public void onPrepared(MediaPlayer mp) {
                mp.setLooping(true);
            }
        });
        Uri uri = Uri.parse("android.resource://" + getPackageName() + "/" + R.raw.loading);
        videoview.setVideoURI(uri);
        videoview.start();

        tasks.add(() -> {
            GlobalVariables.bl.getEnclosures(new GetObjectInterface() {
                @Override
                public void onSuccess(Object response) {
                    GlobalVariables.testEnc = (Enclosure[]) response;
                    doneSignal.countDown();
                }

                @Override
                public void onFailure(Object response) {
                    Log.e("Can't make task", (String) response);
                }
            });
        });

        tasks.add(() -> {
            GlobalVariables.bl.getOpeningHours(new GetObjectInterface() {
                @Override
                public void onSuccess(Object response) {
                    GlobalVariables.testOp = (OpeningHours []) response;
                    doneSignal.countDown();
                }

                @Override
                public void onFailure(Object response) {
                    Log.e("Can't make task", (String) response);
                }
            });
        });

        tasks.add(() -> {
            changeToHebrew();
            doneSignal.countDown();
        });

        makeTasks();

    }


    private void goToMain(){
        Intent loadingIntent = new Intent(LoadingScreen.this, MainActivity.class);
        startActivity(loadingIntent);
    }


    private void changeToHebrew() {
        if (GlobalVariables.firstEnter == 0) {
            GlobalVariables.firstEnter++;
            Locale myLocale = new Locale("iw");
            Resources res = getResources();
            DisplayMetrics dm = res.getDisplayMetrics();
            Configuration conf = res.getConfiguration();
            conf.locale = myLocale;
            res.updateConfiguration(conf, dm);
        }
    }


    private void makeTasks(){
        doneSignal = new CountDownLatch(tasks.size());
        for (FunctionInterface f : tasks){
            Thread task = new Thread(new Runnable() {
                @Override
                public void run() {
                    f.whatToDo();
                }
            });
            task.start();
        }

        Thread task = new Thread(new Runnable() {
            @Override
            public void run() {
                try {
                    doneSignal.await();
                } catch (InterruptedException e) {
                    e.printStackTrace();
                }
                goToMain();
            }
        });
        task.start();
    }
}
