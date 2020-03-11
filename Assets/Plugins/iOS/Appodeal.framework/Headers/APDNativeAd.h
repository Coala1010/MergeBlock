//
//  APDNativeAd.h
//  Appodeal
//
//  AppodealSDK version 2.5.15
//
//  Copyright © 2020 Appodeal, Inc. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <Appodeal/APDImage.h>
#import <UIKit/UIKit.h>
#import <Appodeal/APDNativeAdViewProtocol.h>



@class APDNativeAd;
/**
 Declaration of native presentation delegate
 */
@protocol APDNativeAdPresentationDelegate <NSObject>

@optional
/**
 Called in case if banner view was successfully loaded,
 but ad expire before show
 @param nativeAd Expired native ad
 */
- (void)nativeAdDidExpired:(nonnull APDNativeAd *)nativeAd;
/**
 Method called after any native ad was show
 @param nativeAd On screen native ad
 */
- (void)nativeAdWillLogImpression:(nonnull APDNativeAd *)nativeAd;
/**
 Call when user taps on native ad
 @param nativeAd Interacted native ad
 */
- (void)nativeAdWillLogUserInteraction:(nonnull APDNativeAd *)nativeAd;

@end
/**
 Instance of this class contains ad data
 */
@interface APDNativeAd : NSObject
/**
 Native ad presentation delegate
 */
@property (nonatomic, weak, nullable) id <APDNativeAdPresentationDelegate> delegate;
/**
 Ad title, required field to display. Length less than or equal to about 120 characters.
 */
@property (copy, nonatomic, readonly, nonnull) NSString * title;
/**
 Ad description, optional field to display. Length less than or equal to about 400 characters.
 */
@property (copy, nonatomic, readonly, nonnull) NSString *descriptionText;
/**
 Ad call to action text, required field to display. Length less than or equal to about 120 characters.
 */
@property (copy, nonatomic, readonly, nonnull) NSString *callToActionText;
/**
 Ad content rating to action text, optional field to display. Length less than or equal to about 120 characters.
 */
@property (copy, nonatomic, readonly, nullable) NSString *contentRating;
/**
 Rating of advertised app, optional field.
 */
@property (copy, nonatomic, readonly, nullable) NSNumber *starRating;
/**
 Main image from native ad, optional field. Prevalent aspect ratio is 16:9.
 */
@property (copy, nonatomic, readonly, nullable) APDImage *mainImage;
/**
 Square icon image, required field. Prevalent sizes 50x50, 80x80.
 */
@property (copy, nonatomic, readonly, nullable) APDImage *iconImage;
/**
 Ad Choices view. Can be nil. Provided by ad network. If it contains data, required to display. Minimum size 24x24.
 */
@property (nonatomic, strong, readonly, nullable) UIView * adChoicesView;
/**
 Gets that native ad contains video
 */
@property (nonatomic, readonly, getter=isContainsVideo) BOOL containsVideo;
/**
 Get predicated ecpm
 */
@property (assign, nonatomic, readonly) double predictedEcpm;
/**
 Return instance of UIView subclass that was specified in APDNativeAdSettings
 returned view is ready for show
 @param controller Controller for present modal controller when user tap on ad
 @return instance of UIView
 */
- (nullable UIView <APDNativeAdView> *)getAdViewForController:(nonnull UIViewController *)controller;
/**
 Check availability to show with adjusted placement
 @param placement Placement name string
 @return Can show value
 */
- (BOOL)canShowAdForPlacement:(nonnull NSString *)placement;
/**
 Return instance of UIView subclass that was specified in APDNativeAdSettings
 @param placement Placement name string
 @param controller Controller for present modal controller when user tap on ad
 @param error Error
 @return View is ready for show
 */
- (nullable UIView <APDNativeAdView> *)getAdViewForPlacement:(nonnull NSString *)placement
                                      withRootViewController:(nonnull UIViewController *)controller
                                                       error:(NSError * _Nullable __autoreleasing * _Nullable)error;

@end
