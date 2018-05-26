package com.zoovisitors.backend;

import com.google.gson.annotations.SerializedName;

public class OpeningHoursResult {
    @SerializedName("openingHours")
    private OpeningHours[] openingHours;
    @SerializedName("openingHoursNote")
    private String openingHoursNote;

    public class OpeningHours {
        @SerializedName("day")
        private String day;
        @SerializedName("startTime")
        private String startTime;
        @SerializedName("endTime")
        private String endTime;
    }
}