package com.zoovisitors.pl.animals;


import android.graphics.Typeface;
import android.os.Bundle;
import android.support.v4.app.Fragment;
import android.util.Log;
import android.view.Gravity;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

import com.zoovisitors.GlobalVariables;
import com.zoovisitors.R;
import com.zoovisitors.backend.Animal;

import java.util.HashMap;
import java.util.Map;


/**
 * A simple {@link Fragment} subclass.
 */
public class aboutSpeciesFragment extends Fragment {

    private Animal animal;
    private LinearLayout linearLayoutSpecies;
    private Map<Integer, Integer> numToPicture;

    public aboutSpeciesFragment() {
        // Required empty public constructor
    }


    @Override
    public View onCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState) {
        View rootView = inflater.inflate(R.layout.about_species_res, container, false);

//        numToPicture = new HashMap<>();
//        numToPicture.put(1, R.mipmap.conservation8);
//        numToPicture.put(2, R.mipmap.conservation6);
//        numToPicture.put(3, R.mipmap.conservation5);
//        numToPicture.put(4, R.mipmap.conservation4);
//        numToPicture.put(5, R.mipmap.conservation3);
//        numToPicture.put(6, R.mipmap.conservation2);
//        numToPicture.put(7, R.mipmap.conservation1);

//        ((ImageView) rootView.findViewById(R.id.conservation_image)).setImageResource(numToPicture.get(animal.getPreservation()));
        linearLayoutSpecies = (LinearLayout) rootView.findViewById(R.id.linear_about_species);

//        if (animal.getPreservation() >= 1 && animal.getPreservation() <= 7)
//             linearLayoutSpecies.addView(createLinearLayoutPicture(GlobalVariables.appCompatActivity.getString(R.string.preservation),
//                                    numToPicture.get(animal.getPreservation())));
        linearLayoutSpecies.addView(createLinearLayout(GlobalVariables.appCompatActivity.getString(R.string.category),
                animal.getCategory()));
        linearLayoutSpecies.addView(createLinearLayout(GlobalVariables.appCompatActivity.getString(R.string.series),
                animal.getSeries()));
        linearLayoutSpecies.addView(createLinearLayout(GlobalVariables.appCompatActivity.getString(R.string.family),
                animal.getFamily()));
        linearLayoutSpecies.addView(createLinearLayout(GlobalVariables.appCompatActivity.getString(R.string.distribution),
                animal.getDistribution()));
        linearLayoutSpecies.addView(createLinearLayout(GlobalVariables.appCompatActivity.getString(R.string.reproduction),
                animal.getReproduction()));
        linearLayoutSpecies.addView(createLinearLayout(GlobalVariables.appCompatActivity.getString(R.string.food),
                animal.getFood()));
        return rootView;
    }


    private LinearLayout createLinearLayout(String headline, String value){
        Log.e("ANIMAL", headline);
        if (headline.equals("null"))
            headline = getResources().getString(R.string.no_data);
        LinearLayout linearLayout = new LinearLayout(GlobalVariables.appCompatActivity);
        linearLayout.setOrientation(LinearLayout.VERTICAL);
        TextView headlineTextView = new TextView(GlobalVariables.appCompatActivity);
        TextView valueTextView = new TextView(GlobalVariables.appCompatActivity);
        headlineTextView.setText(headline);
        headlineTextView.setTextSize(15);
        headlineTextView.setTypeface(headlineTextView.getTypeface(), Typeface.BOLD);
        headlineTextView.setTextColor(getResources().getColor(R.color.black));
        valueTextView.setText(" " + value);
        valueTextView.setTextColor(getResources().getColor(R.color.black));

        //insert to layout at order of the language
        if (GlobalVariables.language == 1 || GlobalVariables.language == 3){ //headline on the right
            linearLayout.setGravity(Gravity.RIGHT);
        }
        else //headline on the left
        {
            linearLayout.setGravity(Gravity.LEFT);
        }
        linearLayout.addView(headlineTextView);
        linearLayout.addView(valueTextView);
        linearLayout.addView(new TextView(GlobalVariables.appCompatActivity));
        return linearLayout;
    }

    private LinearLayout createLinearLayoutPicture(String headline, int pic){
        LinearLayout linearLayout = new LinearLayout(GlobalVariables.appCompatActivity);
        linearLayout.setOrientation(LinearLayout.HORIZONTAL);
        TextView headlineTextView = new TextView(GlobalVariables.appCompatActivity);
        ImageView picView = new ImageView(GlobalVariables.appCompatActivity);

        LinearLayout.LayoutParams layoutParams = new LinearLayout.LayoutParams(600, 400);
        picView.setLayoutParams(layoutParams);

        headlineTextView.setText(headline);
        headlineTextView.setTextColor(getResources().getColor(R.color.black));
        picView.setImageResource(pic);
        //insert to layout at order of the language
        if (GlobalVariables.language == 1 || GlobalVariables.language == 3){ //headline on the right
            linearLayout.setGravity(Gravity.RIGHT);
            linearLayout.addView(picView);
            linearLayout.addView(headlineTextView);
        }
        else //headline on the left
        {
            linearLayout.addView(headlineTextView);
            linearLayout.addView(picView);
        }
        return linearLayout;
    }

    public void setAnimal(Animal animal) {
        this.animal = animal;
    }
}
