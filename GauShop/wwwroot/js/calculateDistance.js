<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/3.6.0/jquery.min.js"></script>

	function calculateDistance() {
		var origin = @Model.adminAddress.country, @Model.adminAddress.city,
	@Model.adminAddress.district, @Model.adminAddress.town;
		var destination = document.getElementById("c_country").value, document.getElementById("c_city").value,
			document.getElementById("c_district").value, document.getElementById("c_town").value;

		// Call OpenStreetMap Nominatim API to geocode the addresses
		$.getJSON('https://nominatim.openstreetmap.org/search?format=json&q=' + origin, function (data1) {
			if (data1.length > 0) {
				var originLat = data1[0].lat;
				var originLon = data1[0].lon;

				$.getJSON('https://nominatim.openstreetmap.org/search?format=json&q=' + destination, function (data2) {
					if (data2.length > 0) {
						var destLat = data2[0].lat;
						var destLon = data2[0].lon;

						// Calculate distance between two points using Haversine formula
						var distance = calculateHaversine(originLat, originLon, destLat, destLon);
						var shipping = 0;
						if (distance < 200) {
							shipping = 10;
						} else if (distance >= 200 && distance < 400) {
							shipping = 20;
						} else if (distance >= 400 && distance < 600) {
							shipping = 30;
						} else if (distance >= 600 && distance < 800) {
							shipping = 40;
						} else if (distance >= 800 && distance < 1000) {
							shipping = 50
						} else {
							shipping = 70;
						}
						$('.shipping').text('$' + shipping);
					} else {
						alert('Destination address not found');
					}
				});
			} else {
				alert('Origin address not found');
			}
		});
	}

	function calculateHaversine(lat1, lon1, lat2, lon2) {
		var R = 6371; // Radius of the earth in km
		var dLat = deg2rad(lat2 - lat1);
		var dLon = deg2rad(lon2 - lon1);
		var a =
			Math.sin(dLat / 2) * Math.sin(dLat / 2) +
			Math.cos(deg2rad(lat1)) * Math.cos(deg2rad(lat2)) *
			Math.sin(dLon / 2) * Math.sin(dLon / 2);
		var c = 2 * Math.atan2(Math.sqrt(a), Math.sqrt(1 - a));
		var d = R * c; // Distance in km
		return d;
	}

	function deg2rad(deg) {
		return deg * (Math.PI / 180)
	}
