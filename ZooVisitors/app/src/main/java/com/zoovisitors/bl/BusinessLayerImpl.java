package com.zoovisitors.bl;

import android.support.v7.app.AppCompatActivity;
import android.util.Log;

import com.google.gson.Gson;
import com.zoovisitors.GlobalVariables;
import com.zoovisitors.backend.AboutZoo;
import com.zoovisitors.backend.Animal;
import com.zoovisitors.backend.Enclosure;
import com.zoovisitors.backend.NewsFeed;
import com.zoovisitors.backend.Species;
import com.zoovisitors.cl.network.NetworkImpl;
import com.zoovisitors.cl.network.NetworkInterface;
import com.zoovisitors.cl.network.ResponseInterface;

/**
 * Created by Gili on 13/01/2018.
 */

public class BusinessLayerImpl implements BusinessLayer {

    private NetworkInterface ni;
    private Gson gson;
    private String json;

    public BusinessLayerImpl(AppCompatActivity appCompatActivity) {
        ni = new NetworkImpl(appCompatActivity);
        gson = new Gson();
    }


    @Override
    public void getAnimals(int id, final GetObjectInterface goi) {

        // TODO: example for how to retrieve data from the network, be aware that you should update your ip in GlobalVariables class.
        Log.e("ID", "" + id);
        ni.post("/animals/enclosure/" + id + "/" + GlobalVariables.language, new ResponseInterface() {
            @Override
            public void onSuccess(String response) {
                Animal[] animals = gson.fromJson(response, Animal[].class);
                goi.onSuccess(animals);
            }

            @Override
            public void onFailure(String response) {
                goi.onFailure(null);
            }
        });



        //getJson("animals/1");
    }

    @Override
    public void getEnclosures(final GetObjectInterface goi) {
        ni.post("enclosures/" + GlobalVariables.language, new ResponseInterface() {
            @Override
            public void onSuccess(String response) {
                Enclosure[] enc = gson.fromJson(response, Enclosure[].class);
                goi.onSuccess(enc);
            }

            @Override
            public void onFailure(String response) {
                Log.e("ENCLOSURES", "Can't get enclosures from server");
            }
        });
    }

    @Override
    public void getAboutZoo() {;
        AboutZoo aboutZoo = gson.fromJson(json, AboutZoo.class);
    }

    @Override
    public void getNewsFeed(final GetObjectInterface goi) {

        ni.post("wallfeed/1", new ResponseInterface() {



            @Override
            public void onSuccess(String response) {
                NewsFeed[] newsFeed = gson.fromJson(response, NewsFeed[].class);
                goi.onSuccess(newsFeed);
            }

            @Override
            public void onFailure(String response) {
                Log.e("FEED", "Can't get feed from server");
            }
        });
    }

    @Override
    public void getSpecies() {
        Species species = gson.fromJson(json, Species.class);
    }
}
