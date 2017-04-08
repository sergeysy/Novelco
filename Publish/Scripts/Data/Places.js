var PlacesViewModel = function (source, options) {
	var model = this;

	model.Options = $.extend({
		GetSubRegionsItemsUrl: null,
		GetCountriesItemsUrl: null,
		GetRegionsItemsUrl: null,
		GetPlacesItemsUrl: null,
		GetNewPlaceUrl: null,
		SaveSubRegionUrl: null,
		SaveCountryUrl: null,
		SaveRegionUrl: null,
		SavePlaceUrl: null
	}, options);

	// поля ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	// Список записей
	model.CountriesItems = ko.mapping.fromJS(source.Items);
	model.SubRegionsItems = ko.observableArray();
	model.RegionsItems = ko.observableArray();
	model.PlacesItems = ko.observableArray();
	//
	model.CountriesFilter = null;
	model.SubRegionsFilter = null;
	model.RegionsFilter = null;
	model.PlacesFilter = null;
	//
	model.SelectedCountry = ko.observable();
	model.SelectedSubRegion = ko.observable();
	model.SelectedRegion = ko.observable();

	// вычисляемое/подписки ///////////////////////////////////////////////////////////////////////////////////////////////////


	// функции ////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.SelectCountry = function (country) {
		model.SelectedCountry(country);
		model.SelectedRegion(null);
		//model.ServiceTypesItems([]);

		model.GetRegions(country.ID);
		model.GetPlaces(country.ID, null, null);
	};

	model.SelectRegion = function (region) {
		model.SelectedRegion(region);
		//model.ServiceTypesItems([]);

		model.GetSubRegions(region.ID);
		model.GetPlaces(model.SelectedCountry().ID, region.ID, null);
	};

	model.SelectSubRegion = function (subregion) {
		model.SelectedSubRegion(subregion);
		//model.ServiceTypesItems([]);

		model.GetPlaces(model.SelectedCountry().ID, model.SelectedRegion().ID, subregion.ID);
	};

	model.GetRegions = function (countryId) {
		var id = ko.unwrap(countryId);
		model.RegionsFilter.Filter.ParentId(id);
	};

	model.GetSubRegions = function (regionId) {
		var id = ko.unwrap(regionId);
		model.SubRegionsFilter.Filter.ParentId(id);
	};

	model.GetPlaces = function (countryId, regionId, subregionId) {
		var id = ko.unwrap(countryId);
		model.PlacesFilter.Filter.CountryId(id);
		var id = ko.unwrap(regionId);
		model.PlacesFilter.Filter.RegionId(id);
		var id = ko.unwrap(subregionId);
		model.PlacesFilter.Filter.SubRegionId(id);
	};

	model.SaveCountry = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveCountryUrl,
			data: {
				ID: data.ID(),
				Name: data.Name(),
				EnName: data.EnName(),
				IsoCode: data.IsoCode()

				//IsDeleted: data.IsDeleted()
			}
		});
	};

	model.SaveRegion = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveRegionUrl,
			data: {
				ID: data.ID(),
				Name: data.Name(),
				EnName: data.EnName(),
				IsoCode: data.IsoCode()

				//IsDeleted: data.IsDeleted()
			}
		});
	};

	model.SaveSubRegion = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SaveSubRegionUrl,
			data: {
				ID: data.ID(),
				Name: data.Name(),
				EnName: data.EnName()

				//IsDeleted: data.IsDeleted()
			}
		});
	};

	model.SavePlace = function (data) {
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.SavePlaceUrl,
			data: {
				ID: data.ID(),
				Name: data.Name(),
				EnName: data.EnName(),
				IataCode: data.IataCode(),
				IcaoCode: data.IcaoCode(),
				Airport: data.Airport(),
				CountryId: data.CountryId(),
				RegionId: data.RegionId(),
				SubRegionId: data.SubRegionId()
			},
			success: function (response) {
				var r = JSON.parse(response);
				if (r.Message) {
					alert(r.Message);
				}
				else {
					if (data.ID() == 0)
						data.ID(r.ID);
				}
			}
		});
	};

	// #region country create/edit modal //////////////////////////////////////////////////////////////////////////////////////

	var countryModalSelector = "#countryEditModal";

	model.OpenCountryEdit = function (country) {
		model.CountryEditModal.CurrentItem(country);
		$(countryModalSelector).modal("show");
		//$(countryModalSelector).draggable({ handle: ".modal-header" });
		model.CountryEditModal.OnClosed = null;
		model.CountryEditModal.Init();
	};

	model.CountryEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e) {
			$(countryModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			model.SaveCountry(self.CurrentItem());
			self.CurrentItem(null);
			$(countryModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region region create/edit modal ///////////////////////////////////////////////////////////////////////////////////////

	var regionModalSelector = "#regionEditModal";

	model.OpenRegionEdit = function (region) {
		model.RegionEditModal.CurrentItem(region);
		$(regionModalSelector).modal("show");
		//$(countryModalSelector).draggable({ handle: ".modal-header" });
		model.RegionEditModal.OnClosed = null;
		model.RegionEditModal.Init();
	};

	model.RegionEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e) {
			$(regionModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			model.SaveRegion(self.CurrentItem());
			self.CurrentItem(null);
			$(regionModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region subregion create/edit modal ////////////////////////////////////////////////////////////////////////////////////

	var subRegionModalSelector = "#subRegionEditModal";

	model.OpenSubRegionEdit = function (subregion) {
		model.SubRegionEditModal.CurrentItem(subregion);
		$(subRegionModalSelector).modal("show");
		//$(countryModalSelector).draggable({ handle: ".modal-header" });
		model.SubRegionEditModal.OnClosed = null;
		model.SubRegionEditModal.Init();
	};

	model.SubRegionEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e) {
			$(subRegionModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			model.SaveSubRegion(self.CurrentItem());
			self.CurrentItem(null);
			$(subRegionModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region place create/edit modal ////////////////////////////////////////////////////////////////////////////////////////

	var placeModalSelector = "#placeEditModal";

	model.OpenPlaceCreate = function (data) {
		var data = null;
		$.ajax({
			type: "POST",
			async: false,
			url: model.Options.GetNewPlaceUrl,
			data: { CountryId: model.SelectedCountry().ID(), RegionId: (model.SelectedRegion()) ? model.SelectedRegion().ID() : null, SubRegionId: (model.SelectedSubRegion()) ? model.SelectedSubRegion().ID() : null },
			success: function (response) {
				var r = JSON.parse(response);
				if (r.Message)
					alert(r.Message);
				else {
					data = ko.mapping.fromJSON(response);
					model.PlacesItems.push(data);
				}
			}
		});

		if (!data)
			return;

		model.PlaceEditModal.CurrentItem(data);
		$(placeModalSelector).modal("show");
		//$(placeModalSelector).draggable({ handle: ".modal-header" });
		model.PlaceEditModal.Init();
	};

	model.OpenPlaceEdit = function (region) {
		model.PlaceEditModal.CurrentItem(region);
		$(placeModalSelector).modal("show");
		//$(countryModalSelector).draggable({ handle: ".modal-header" });
		model.PlaceEditModal.OnClosed = null;
		model.PlaceEditModal.Init();
	};

	model.PlaceEditModal = {
		CurrentItem: ko.observable(),
		Init: function () { },
		OnClosed: null,
		Close: function (self, e) {
			$(placeModalSelector).modal("hide");
			if (self.OnClosed != null)
				self.OnClosed();
		},
		Done: function (self, e) {
			model.SavePlace(self.CurrentItem());
			self.CurrentItem(null);
			$(placeModalSelector).modal("hide");
		}
	};

	// #endregion

	// #region Фильтры ////////////////////////////////////////////////////////////////////////////////////////////////////////

	model._CountriesFilter = function () {
		var self = this;

		self.Filter = ko.mapping.fromJS(source.Filter);
		// Общее количество записей для текущего фильтра
		self.TotalItemsCount = ko.observable(source.TotalItemsCount);

		// computed
		self.TotalPageCount = ko.computed(function () { return Math.ceil(self.TotalItemsCount() / self.Filter.PageSize()); });
		//
		self.Filter.Context.subscribe(function () { self.ApplyFilter() });

		// функции
		self.SortBy = function (field) {
			var f = ko.unwrap(field);
			if (self.Filter.Sort() == f)
				self.Filter.SortDirection(self.Filter.SortDirection() == "Asc" ? "Desc" : "Asc");
			else
				self.Filter.Sort(f);

			self.ApplyFilter();
		};

		self.ApplyFilter = function () {
			$.ajax({
				type: "POST",
				url: model.Options.GetCountriesItemsUrl,
				data: {
					Context: self.Filter.Context() || "",
					PageSize: self.Filter.PageSize(),
					PageNumber: self.Filter.PageNumber(),
					Sort: self.Filter.Sort(),
					SortDirection: self.Filter.SortDirection()
				},
				success: function (response) {
					var temp = ko.mapping.fromJSON(response);
					model.CountriesItems(temp.Items());
					self.TotalItemsCount(temp.TotalCount());
					if (self.Filter.PageNumber() > self.TotalPageCount())
						self.Filter.PageNumber(0);
				}
			});
		};

		self.FirstPage = function () {
			if (self.Filter.PageNumber() > 0) {
				self.Filter.PageNumber(0);
				self.ApplyFilter();
			}
		};
		self.PrevPage = function () {
			if (self.Filter.PageNumber() > 0) {
				self.Filter.PageNumber(self.Filter.PageNumber() - 1);
				self.ApplyFilter();
			}
		};
		self.NextPage = function () {
			if (self.Filter.PageNumber() < (self.TotalPageCount() - 1)) {
				self.Filter.PageNumber(self.Filter.PageNumber() + 1);
				self.ApplyFilter();
			}
		};
		self.LastPage = function () {
			if (self.Filter.PageNumber() < (self.TotalPageCount() - 1)) {
				self.Filter.PageNumber(self.TotalPageCount() - 1);
				self.ApplyFilter();
			}
		};
	};

	model._RegionsFilter = function () {
		var self = this;

		self.Filter = ko.mapping.fromJS({ Sort: null, SortDirection: null, PageSize: 8, PageNumber: 0, Context: "", ParentId: null });
		// Общее количество записей для текущего фильтра
		self.TotalItemsCount = ko.observable(0);

		// computed
		self.TotalPageCount = ko.computed(function () { return Math.ceil(self.TotalItemsCount() / self.Filter.PageSize()); });
		//
		self.Filter.Context.subscribe(function () { self.ApplyFilter() });
		self.Filter.ParentId.subscribe(function () { self.ApplyFilter() });

		// функции
		self.SortBy = function (field) {
			var f = ko.unwrap(field);
			if (self.Filter.Sort() == f)
				self.Filter.SortDirection(self.Filter.SortDirection() == "Asc" ? "Desc" : "Asc");
			else
				self.Filter.Sort(f);

			self.ApplyFilter();
		};

		self.ApplyFilter = function () {
			$.ajax({
				type: "POST",
				url: model.Options.GetRegionsItemsUrl,
				data: {
					ParentId: self.Filter.ParentId() || 0,
					Context: self.Filter.Context() || "",
					PageSize: self.Filter.PageSize(),
					PageNumber: self.Filter.PageNumber(),
					Sort: self.Filter.Sort(),
					SortDirection: self.Filter.SortDirection()
				},
				success: function (response) {
					var temp = ko.mapping.fromJSON(response);
					model.RegionsItems(temp.Items());
					self.TotalItemsCount(temp.TotalCount());
					if (self.Filter.PageNumber() > self.TotalPageCount())
						self.Filter.PageNumber(0);
				}
			});
		};

		self.FirstPage = function () {
			if (self.Filter.PageNumber() > 0) {
				self.Filter.PageNumber(0);
				self.ApplyFilter();
			}
		};
		self.PrevPage = function () {
			if (self.Filter.PageNumber() > 0) {
				self.Filter.PageNumber(self.Filter.PageNumber() - 1);
				self.ApplyFilter();
			}
		};
		self.NextPage = function () {
			if (self.Filter.PageNumber() < (self.TotalPageCount() - 1)) {
				self.Filter.PageNumber(self.Filter.PageNumber() + 1);
				self.ApplyFilter();
			}
		};
		self.LastPage = function () {
			if (self.Filter.PageNumber() < (self.TotalPageCount() - 1)) {
				self.Filter.PageNumber(self.TotalPageCount() - 1);
				self.ApplyFilter();
			}
		};
	};

	model._SubRegionsFilter = function () {
		var self = this;

		self.Filter = ko.mapping.fromJS({ Sort: null, SortDirection: null, PageSize: 8, PageNumber: 0, Context: "", ParentId: null });
		// Общее количество записей для текущего фильтра
		self.TotalItemsCount = ko.observable(0);

		// computed
		self.TotalPageCount = ko.computed(function () { return Math.ceil(self.TotalItemsCount() / self.Filter.PageSize()); });
		//
		self.Filter.Context.subscribe(function () { self.ApplyFilter() });
		self.Filter.ParentId.subscribe(function () { self.ApplyFilter() });

		// функции
		self.SortBy = function (field) {
			var f = ko.unwrap(field);
			if (self.Filter.Sort() == f)
				self.Filter.SortDirection(self.Filter.SortDirection() == "Asc" ? "Desc" : "Asc");
			else
				self.Filter.Sort(f);

			self.ApplyFilter();
		};

		self.ApplyFilter = function () {
			$.ajax({
				type: "POST",
				url: model.Options.GetSubRegionsItemsUrl,
				data: {
					ParentId: self.Filter.ParentId() || 0,
					Context: self.Filter.Context() || "",
					PageSize: self.Filter.PageSize(),
					PageNumber: self.Filter.PageNumber(),
					Sort: self.Filter.Sort(),
					SortDirection: self.Filter.SortDirection()
				},
				success: function (response) {
					var temp = ko.mapping.fromJSON(response);
					model.SubRegionsItems(temp.Items());
					self.TotalItemsCount(temp.TotalCount());
					if (self.Filter.PageNumber() > self.TotalPageCount())
						self.Filter.PageNumber(0);
				}
			});
		};

		self.FirstPage = function () {
			if (self.Filter.PageNumber() > 0) {
				self.Filter.PageNumber(0);
				self.ApplyFilter();
			}
		};
		self.PrevPage = function () {
			if (self.Filter.PageNumber() > 0) {
				self.Filter.PageNumber(self.Filter.PageNumber() - 1);
				self.ApplyFilter();
			}
		};
		self.NextPage = function () {
			if (self.Filter.PageNumber() < (self.TotalPageCount() - 1)) {
				self.Filter.PageNumber(self.Filter.PageNumber() + 1);
				self.ApplyFilter();
			}
		};
		self.LastPage = function () {
			if (self.Filter.PageNumber() < (self.TotalPageCount() - 1)) {
				self.Filter.PageNumber(self.TotalPageCount() - 1);
				self.ApplyFilter();
			}
		};
	};

	model._PlacesFilter = function () {
		var self = this;

		self.Filter = ko.mapping.fromJS({ Sort: null, SortDirection: null, PageSize: 8, PageNumber: 0, Context: "", CountryId: null, RegionId: null, SubRegionId: null });
		// Общее количество записей для текущего фильтра
		self.TotalItemsCount = ko.observable(0);

		// computed
		self.TotalPageCount = ko.computed(function () { return Math.ceil(self.TotalItemsCount() / self.Filter.PageSize()); });
		//
		self.Filter.Context.subscribe(function () { self.ApplyFilter() });
		self.Filter.CountryId.subscribe(function () { self.ApplyFilter() });
		self.Filter.RegionId.subscribe(function () { self.ApplyFilter() });
		self.Filter.SubRegionId.subscribe(function () { self.ApplyFilter() });

		// функции
		self.SortBy = function (field) {
			var f = ko.unwrap(field);
			if (self.Filter.Sort() == f)
				self.Filter.SortDirection(self.Filter.SortDirection() == "Asc" ? "Desc" : "Asc");
			else
				self.Filter.Sort(f);

			self.ApplyFilter();
		};

		self.ApplyFilter = function () {
			$.ajax({
				type: "POST",
				url: model.Options.GetPlacesItemsUrl,
				data: {
					CountryId: self.Filter.CountryId() || 0,
					RegionId: self.Filter.RegionId() || 0,
					SubRegionId: self.Filter.SubRegionId() || 0,
					Context: self.Filter.Context() || "",
					PageSize: self.Filter.PageSize(),
					PageNumber: self.Filter.PageNumber(),
					Sort: self.Filter.Sort(),
					SortDirection: self.Filter.SortDirection()
				},
				success: function (response) {
					var temp = ko.mapping.fromJSON(response);
					model.PlacesItems(temp.Items());
					self.TotalItemsCount(temp.TotalCount());
					if (self.Filter.PageNumber() > self.TotalPageCount())
						self.Filter.PageNumber(0);
				}
			});
		};

		self.FirstPage = function () {
			if (self.Filter.PageNumber() > 0) {
				self.Filter.PageNumber(0);
				self.ApplyFilter();
			}
		};
		self.PrevPage = function () {
			if (self.Filter.PageNumber() > 0) {
				self.Filter.PageNumber(self.Filter.PageNumber() - 1);
				self.ApplyFilter();
			}
		};
		self.NextPage = function () {
			if (self.Filter.PageNumber() < (self.TotalPageCount() - 1)) {
				self.Filter.PageNumber(self.Filter.PageNumber() + 1);
				self.ApplyFilter();
			}
		};
		self.LastPage = function () {
			if (self.Filter.PageNumber() < (self.TotalPageCount() - 1)) {
				self.Filter.PageNumber(self.TotalPageCount() - 1);
				self.ApplyFilter();
			}
		};
	};

	// #endregion

	// инициализация //////////////////////////////////////////////////////////////////////////////////////////////////////////

	model.CountriesFilter = new model._CountriesFilter();
	model.RegionsFilter = new model._RegionsFilter();
	model.SubRegionsFilter = new model._SubRegionsFilter();
	model.PlacesFilter = new model._PlacesFilter();
}

$(function () {
	ko.applyBindings(new PlacesViewModel(modelData, {
		GetSubRegionsItemsUrl: app.urls.GetSubRegionsItems,
		GetCountriesItemsUrl: app.urls.GetCountriesItems,
		GetRegionsItemsUrl: app.urls.GetRegionsItems,
		GetPlacesItemsUrl: app.urls.GetPlacesItems,
		GetNewPlaceUrl: app.urls.GetNewPlace,
		SaveSubRegionUrl: app.urls.SaveSubRegion,
		SaveCountryUrl: app.urls.SaveCountry,
		SaveRegionUrl: app.urls.SaveRegion,
		SavePlaceUrl: app.urls.SavePlace
	}), document.getElementById("ko-root"));
});